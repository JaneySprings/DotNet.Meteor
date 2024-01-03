using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNet.Meteor.Shared {
    public static class ProjectExtensions {
        public static string EvaluateProperty(this Project project, string name, string defaultValue = null) {
            var propertyMatches = project.GetPropertyMatches(project.Path, name);
            if (propertyMatches == null)
                return defaultValue;

            var propertyValue = project.GetPropertyValue(name, propertyMatches);
            if (string.IsNullOrEmpty(propertyValue))
                return defaultValue;

            return propertyValue;
        }

        private static MatchCollection GetPropertyMatches(this Project project, string projectPath, string propertyName, bool isEndPoint = false) {
            if (!File.Exists(projectPath))
                return null;

            string content = File.ReadAllText(projectPath);
            content = Regex.Replace(content, "<!--.*?-->", string.Empty, RegexOptions.Singleline);
            /* Find in current project */
            var propertyMatch = new Regex($@"<{propertyName}\s?.*>(.*?)<\/{propertyName}>\s*\n").Matches(content);
            if (propertyMatch.Count > 0)
                return propertyMatch;
            var importRegex = new Regex(@"<Import\s+Project\s*=\s*""(.*?)""");
            /* Find in imported project */
            foreach(Match importMatch in importRegex.Matches(content)) {
                var importedProjectPath = ResolveImportPath(projectPath, importMatch.Groups[1].Value);
                if (!File.Exists(importedProjectPath))
                    return null;

                var importedProjectPropertyMatches = project.GetPropertyMatches(importedProjectPath, propertyName, isEndPoint);
                if (importedProjectPropertyMatches != null)
                    return importedProjectPropertyMatches;
            }
            /* Already at the end of the import chain */
            if (isEndPoint)
                return null;
            /* Find in Directory.Build.props */
            var propsFile = GetDirectoryPropsPath(Path.GetDirectoryName(projectPath)!);
            if (propsFile == null)
                return null;

            return project.GetPropertyMatches(propsFile, propertyName, true);
        }

        private static string GetPropertyValue(this Project project, string propertyName, MatchCollection matches) {
            var includeRegex = new Regex(@"\$\((?<inc>.*?)\)");
            var resultSequence = new StringBuilder();
            /* Process all property entrance */
            foreach (Match match in matches) {
                var propertyValue = match.Groups[1].Value;
                /* If property reference self */
                if (propertyValue.Contains($"$({propertyName})")) {
                    propertyValue = propertyValue.Replace($"$({propertyName})", resultSequence.ToString());
                    resultSequence.Clear();
                }
                /* If property reference other property */
                foreach (Match includeMatch in includeRegex.Matches(propertyValue)) {
                    var includePropertyName = includeMatch.Groups["inc"].Value;
                    var includePropertyValue = project.EvaluateProperty(includePropertyName);
                    propertyValue = propertyValue.Replace($"$({includePropertyName})", includePropertyValue ?? "");
                }
                /* Add separator and property to builder */
                if (resultSequence.Length != 0)
                    resultSequence.Append(';');
                resultSequence.Append(propertyValue);
            }
            return resultSequence.ToString();
        }

        public static string FindOutputApplication(this Project project, string configuration, DeviceData device, Func<string, string> errorHandler = null) {
            var framework = project.Frameworks.FirstOrDefault(it => it.ContainsInsensitive(device.Platform));
            if (string.IsNullOrEmpty(framework))
                return errorHandler?.Invoke($"Could not find targetFramework for platform {device.Platform}");

            var rootDirectory = Path.GetDirectoryName(project.Path);
            var baseOutputDirectory = Path.Combine(rootDirectory, "bin", configuration, framework);
            var outputAssemblyPath = string.Empty;

            if (!string.IsNullOrEmpty(device.RuntimeId)) {
                var baseOutputDirectoryWithRuntimeId = Path.Combine(baseOutputDirectory, device.RuntimeId);
                outputAssemblyPath = FindOutputApplicationWithDirectoryPath(baseOutputDirectoryWithRuntimeId, project, device, errorHandler);
                if (!string.IsNullOrEmpty(outputAssemblyPath))
                    return outputAssemblyPath;
            }

            outputAssemblyPath = FindOutputApplicationWithDirectoryPath(baseOutputDirectory, project, device, errorHandler);
            if (string.IsNullOrEmpty(outputAssemblyPath))
                return errorHandler?.Invoke($"Could not find output application in {baseOutputDirectory}");

            return outputAssemblyPath;
        }

        private static string FindOutputApplicationWithDirectoryPath(string directoryPath, Project project, DeviceData device, Func<string, string> errorHandler = null) {
            if (!Directory.Exists(directoryPath))
                return errorHandler?.Invoke($"Could not find output directory {directoryPath}");

            if (device.IsAndroid) {
                var files = Directory.GetFiles(directoryPath, "*-Signed.apk", SearchOption.TopDirectoryOnly);
                if (files.Length > 1)
                    return errorHandler?.Invoke($"Found more than one \"*-Signed.apk\" in {directoryPath}");
                return files.FirstOrDefault();
            }

            if (device.IsWindows) {
                var executableName = project.EvaluateProperty("AssemblyName", project.Name);
                var files = Directory.GetFiles(directoryPath, $"{executableName}.exe", SearchOption.AllDirectories);
                if (files.Length > 1)
                    return errorHandler?.Invoke($"Found more than one \"{executableName}.exe\" in {directoryPath} and subdirectories");
                return files.FirstOrDefault();
            }

            if (device.IsIPhone || device.IsMacCatalyst) {
                var bundle = Directory.GetDirectories(directoryPath, "*.app", SearchOption.AllDirectories);
                if (bundle.Length > 1)
                    return errorHandler?.Invoke($"Found more than one \"*.app\" in {directoryPath} and subdirectories");
                return bundle.FirstOrDefault();
            }

            return null;
        }

        private static string GetDirectoryPropsPath(string workspacePath) {
            var propFiles = Directory.GetFiles(workspacePath, "Directory.Build.props", SearchOption.TopDirectoryOnly);
            if (propFiles.Length > 0)
                return propFiles[0];

            var parentDirectory = Directory.GetParent(workspacePath);
            if (parentDirectory == null)
                return null;

            return GetDirectoryPropsPath(parentDirectory.FullName);
        }

        private static string ResolveImportPath(string filePath, string importPath) {
            var thisFileDirectory = Path.GetDirectoryName(filePath);
            var importFilePath = importPath.Replace("$(MSBuildThisFileDirectory)", thisFileDirectory + Path.DirectorySeparatorChar);
            if (Path.IsPathRooted(importFilePath))
                return Path.GetFullPath(importFilePath).ToPlatformPath();

            return Path.GetFullPath(Path.Combine(thisFileDirectory, importFilePath)).ToPlatformPath();
        }
    }
}
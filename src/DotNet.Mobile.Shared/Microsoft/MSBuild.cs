using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNet.Mobile.Shared {
    public static class MSBuild {
        public static string GetProperty(string projectPath, string propertyName) {
            var matches = GetPropertyInternal(projectPath, propertyName, false);
            if (matches == null)
                return null;
            return EvaluateProperty(projectPath, matches, propertyName);
        }

        private static MatchCollection GetPropertyInternal(string projectPath, string propertyName, bool isEndPoint = false) {
            if (!File.Exists(projectPath))
                return null;

            string content = File.ReadAllText(projectPath);
            // Find in current project
            var propertyMatch = new Regex($@"<{propertyName}\s?.*>(.*?)<\/{propertyName}>\s*\n").Matches(content);
            if (propertyMatch.Count > 0)
                return propertyMatch;
            // Find in imported project
            var importMatch = new Regex(@"<Import\s+Project\s*=\s*""(.*?)""").Match(content);
            if (importMatch.Success) {
                var importPath = NormalizePath(Path.Combine(Path.GetDirectoryName(projectPath), importMatch.Groups[1].Value));
                var importResult = GetPropertyInternal(importPath, propertyName, isEndPoint);

                if (importResult != null)
                    return importResult;
            }
            // Already at the end of the import chain
            if (isEndPoint)
                return null;
            // Find in Directory.Build.props
            var propsFile = GetDirectoryPropsPath(Path.GetDirectoryName(projectPath));
            if (propsFile == null)
                return null;

            return GetPropertyInternal(propsFile, propertyName, true);
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

        private static string NormalizePath(string path) {
            return path.Replace('\\', Path.DirectorySeparatorChar);
        }

        private static string EvaluateProperty(string projectPath, MatchCollection matches, string propertyName) {
            var regex = new Regex(@"\$\((?<inc>.*?)\)");
            var resultSequence = new StringBuilder();
            foreach (Match match in matches) {
                var property = match.Groups[1].Value;
                if (property.Contains($"$({propertyName})")) {
                    property = property.Replace($"$({propertyName})", resultSequence.ToString());
                    resultSequence.Clear();
                }

                foreach (Match includeMatch in regex.Matches(property)) {
                    var include = includeMatch.Groups["inc"].Value;
                    var includeProperty = GetProperty(projectPath, include);
                    property = property.Replace($"$({include})", includeProperty ?? "");
                }
                if (resultSequence.Length != 0)
                    resultSequence.Append(";");
                resultSequence.Append(property);
            }
            return resultSequence.ToString();
        }
    }
}
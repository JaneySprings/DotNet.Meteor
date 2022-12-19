using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace DotNet.Mobile.Debug;

public static class WorkspaceAnalyzer {
    public static IEnumerable<Project> AnalyzeWorkspace(string workspacePath) {
        var projects = new List<Project>();
        var projectFiles = Directory.GetFiles(workspacePath, "*.csproj", SearchOption.AllDirectories);

        foreach (var projectFile in projectFiles) {
            if (!GetIsExecutable(projectFile))
                continue;

            var project = AnalyzeProject(projectFile);
            if (project.Frameworks?.Find(it => it.Contains("net", System.StringComparison.OrdinalIgnoreCase) && it.Contains('-')) != null)
                projects.Add(project);
        }

        return projects.OrderBy(x => x.Name);
    }

    public static Project AnalyzeProject(string projectFile) {
        return new Project {
            Name = Path.GetFileNameWithoutExtension(projectFile),
            Frameworks = GetTargetFrameworks(projectFile),
            Path = projectFile
        };
    }



    private static List<string> GetTargetFrameworks(string projectPath) {
        var frameworks = new List<string>();

        var singleFramework = GetProperty(projectPath, "TargetFramework");
        if (!string.IsNullOrEmpty(singleFramework)) {
            frameworks.Add(singleFramework);
            return frameworks;
        }

        var multipleFrameworks = GetProperty(projectPath, "TargetFrameworks");
        if (!string.IsNullOrEmpty(multipleFrameworks)) {
            foreach (var framework in multipleFrameworks.Split(';')) {
                if (frameworks.Contains(framework) || framework.StartsWith("$("))
                    continue;
                frameworks.Add(framework);
            }
            return frameworks;
        }

        return null;
    }

    private static bool GetIsExecutable(string projectPath) {
        string property = GetProperty(projectPath, "OutputType");
        return property?.Contains("exe", System.StringComparison.OrdinalIgnoreCase) == true;
    }

    private static string GetProperty(string projectPath, string propertyName, bool isEndPoint = false) {
        if (!File.Exists(projectPath))
            return null;

        string content = File.ReadAllText(projectPath);
        // Find in current project
        var propertyMatch = new Regex($@"<{propertyName}\s?.*>(.*?)<\/{propertyName}>\s*\n").Matches(content);
        if (propertyMatch.Count > 0) {
            var matches = propertyMatch.Select(it => it.Groups[1].Value).ToList();
            return string.Join(';', matches);
        }
        // Find in imported project
        var importMatch = new Regex(@"<Import\s+Project\s*=\s*""(.*?)""").Match(content);
        if (importMatch.Success) {
            var importPath = NormalizePath(Path.Combine(Path.GetDirectoryName(projectPath), importMatch.Groups[1].Value));
            var importResult = GetProperty(importPath, propertyName, isEndPoint);

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

        return GetProperty(propsFile, propertyName, true);
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
}
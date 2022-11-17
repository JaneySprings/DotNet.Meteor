using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace DotNet.Mobile.Debug;

public static class WorkspaceAnalyzer {
    public static List<Project> GetProjects(string workspacePath) {
        var projects = new List<Project>();
        var projectFiles = Directory.GetFiles(workspacePath, "*.csproj", SearchOption.AllDirectories);

        foreach (var projectFile in projectFiles) {
            if (!GetIsExecutable(projectFile))
                continue;

            projects.Add(new Project {
                Name = Path.GetFileNameWithoutExtension(projectFile),
                Frameworks = GetTargetFrameworks(projectFile),
                Path = projectFile
            });
        }

        return projects;
    }

    private static List<string> GetTargetFrameworks(string projectPath) {
        string property = GetProperty(projectPath, "TargetFrameworks");
        return property?.Split(';')?.Where(it => !string.IsNullOrEmpty(it))?.ToList();
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
        var propertyMatch = new Regex($@"<{propertyName}>(.*?)<\/{propertyName}>").Match(content);
        if (propertyMatch.Success)
            return propertyMatch.Groups[1].Value;
        // Find in imported project
        var importMatch = new Regex(@"<Import\s+Project\s*=\s*""(.*?)""").Match(content);
        if (importMatch.Success) {
            var importPath = Path.Combine(Path.GetDirectoryName(projectPath), importMatch.Groups[1].Value.NormalizePath());
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
}
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace DotNet.Mobile.Shared {

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

            var singleFramework = MSBuild.GetProperty(projectPath, "TargetFramework");
            if (!string.IsNullOrEmpty(singleFramework)) {
                frameworks.Add(singleFramework);
                return frameworks;
            }

            var multipleFrameworks = MSBuild.GetProperty(projectPath, "TargetFrameworks");
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
            string property = MSBuild.GetProperty(projectPath, "OutputType");
            return property?.Contains("exe", System.StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}
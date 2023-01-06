using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace DotNet.Mobile.Shared {

    public static class WorkspaceAnalyzer {
        public static IEnumerable<Project> AnalyzeWorkspace(string workspacePath) {
            var projectFiles = Directory.GetFiles(workspacePath, "*.csproj", SearchOption.AllDirectories);
            var projects = new List<Project>();

            foreach (var projectFile in projectFiles) {
                try {
                    var project = new Project(projectFile);
                    project.Load();

                    string outputType = project.EvaluateProperty("OutputType");
                    if (outputType?.Contains("exe", StringComparison.OrdinalIgnoreCase) == false)
                        continue;

                    project.Frameworks = TargetFrameworks(project);
                    if (project.Frameworks?.Find(it => it.Contains("net", StringComparison.OrdinalIgnoreCase) && it.Contains('-')) != null)
                        projects.Add(project);
                } catch { continue; }
            }

            return projects.OrderBy(x => x.Name);
        }

        public static Project AnalyzeProject(string projectFile) {
            var projects = AnalyzeWorkspace(Path.GetDirectoryName(projectFile));
            return projects.FirstOrDefault(x => x.Path.Equals(projectFile));
        }

        private static List<string> TargetFrameworks(Project project) {
            var frameworks = new List<string>();

            var singleFramework = project.EvaluateProperty("TargetFramework");
            if (!string.IsNullOrEmpty(singleFramework)) {
                frameworks.Add(singleFramework);
                return frameworks;
            }

            var multipleFrameworks = project.EvaluateProperty("TargetFrameworks");
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
    }
}
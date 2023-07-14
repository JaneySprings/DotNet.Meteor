using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Workspace;

public static class WorkspaceAnalyzer {
    public static IEnumerable<Project> AnalyzeWorkspace(string workspacePath, Action<string>? callback = null) {
        var projects = new List<Project>();
        if (!Directory.Exists(workspacePath)) {
            callback?.Invoke($"Could not find workspace directory {workspacePath}");
            return projects;
        }

        foreach (var projectFile in Directory.GetFiles(workspacePath, "*.*proj", SearchOption.AllDirectories)) {
            var project = AnalyzeProject(projectFile, callback);
            if (project == null)
                continue;
            projects.Add(project);
        }

        return projects.OrderBy(x => x.Name);
    }

    public static Project? AnalyzeProject(string projectFile, Action<string>? callback = null) {
        var project = new Project(projectFile);
        var outputType = project.EvaluateProperty("OutputType");

        if (outputType == null || outputType?.Contains("exe", StringComparison.OrdinalIgnoreCase) == false) {
            callback?.Invoke($"Skipping project {project.Name} because it is not an executable.");
            return null;
        }

        project.Frameworks = TargetFrameworks(project);
        if (project.Frameworks?.Find(it => it.Contains("net", StringComparison.OrdinalIgnoreCase) && it.Contains('-')) == null) {
            callback?.Invoke($"Skipping project {project.Name} because it does not contain a valid target framework.");
            return null;
        }

        return project;
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
                if (frameworks.Contains(framework) || string.IsNullOrEmpty(framework))
                    continue;
                frameworks.Add(framework);
            }
            return frameworks;
        }

        return frameworks;
    }
}
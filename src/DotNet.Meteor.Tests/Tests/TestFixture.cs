using Xunit;
using System.Reflection;
using DotNet.Meteor.Shared;
using MSProject = Microsoft.Build.Evaluation.Project;

namespace DotNet.Meteor.Tests;


[Collection("Sequential")]
public abstract class TestFixture {
    protected readonly string MockDataDirectory;

    protected TestFixture() {
        var msbuild = PathUtils.MSBuildAssembly();
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";

        MockDataDirectory = Path.GetFullPath(Path.Combine(assemblyLocation, "..", "..", "..", "..", "MockData"));
        Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", msbuild.FullName);
    }

    protected MSProject EvaluateProject(string projectPath, Dictionary<string, string>? properties = null) {
        return new MSProject(projectPath, properties, null);
    }

    protected string GetProjectPath(int index) {
        return Path.Combine(MockDataDirectory, $"TestApp{index}", $"TestApp{index}.csproj");
    }

    protected void AssertProjects(MSProject expected, Project actual) {
        foreach (var framework in actual.Frameworks) {
            Assert.NotNull(framework);
            Assert.NotEmpty(framework);
        }
        Assert.NotNull(actual);
        Assert.NotNull(actual.Frameworks);
        Assert.NotEmpty(actual.Frameworks);
        Assert.Equal(expected.GetPropertyValue("AssemblyName"), actual.Name);
        Assert.Equal(expected.GetPropertyValue("TargetFrameworks").RemoveEmptyEntries(), actual.Frameworks.Join());
        Assert.Equal(expected.FullPath, actual.Path);
        UnloadProject(expected);
    }

    protected void UnloadProject(MSProject project) {
        Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.UnloadProject(project);
    }
}
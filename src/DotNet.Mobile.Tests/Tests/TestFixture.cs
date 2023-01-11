using Xunit;
using System.Reflection;
using DotNet.Mobile.Shared;
using MSProject = Microsoft.Build.Evaluation.Project;

namespace DotNet.Mobile.Tests;


[Collection("Sequential")]
public abstract class TestFixture {
    protected readonly string MockDataDirectory;

    protected TestFixture() {
        var msbuild = PathUtils.MSBuildAssembly();
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";

        MockDataDirectory = Path.GetFullPath(Path.Combine(assemblyLocation, "..", "..", "..", "..", "MockData"));
        Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", msbuild.FullName);
    }

    protected MSProject EvaluateProject(string projectPath) {
        return new MSProject(projectPath);
    }

    protected string GetProjectPath(int index) {
        return Path.Combine(MockDataDirectory, $"TestApp{index}", $"TestApp{index}.csproj");
    }
}
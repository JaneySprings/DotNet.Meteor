using Xunit;
using DotNet.Meteor.Shared;
using Microsoft.Build.Evaluation;

namespace DotNet.Meteor.Tests;

public class WorkspaceAnalysisTests: TestFixture {

    [Fact]
    public void AnalyzeSimpleProject() {
        var simpleProjectPath = GetProjectPath(index: 1);
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        var expected = EvaluateProject(simpleProjectPath);

        Assert.NotNull(actual);
        Assert.Equal(expected.GetPropertyValue("AssemblyName"), actual.Name);
        Assert.Equal(expected.GetPropertyValue("TargetFramework"), actual.Frameworks.Join());
        Assert.Equal(expected.FullPath, actual.Path);
        UnloadProject(expected);
    }

    [Fact]
    public void AnalyzeProjectWithMultipleFrameworks() {
        var simpleProjectPath = GetProjectPath(index: 2);
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        var expected = EvaluateProject(simpleProjectPath);
        AssertProjects(expected, actual);
    }

    [Fact]
    public void AnalyzeProjectWithFrameworkReference() {
        var simpleProjectPath = GetProjectPath(index: 3);
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        var expected = EvaluateProject(simpleProjectPath);
        AssertProjects(expected, actual);
    }

    [Fact]
    public void AnalyzeProjectWithFrameworkCondition() {
        var simpleProjectPath = GetProjectPath(index: 4);
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        var expected = EvaluateProject(simpleProjectPath);
        AssertProjects(expected, actual);
    }

    [Fact]
    public void AnalyzeProjectWithDirectoryProps() {
        var simpleProjectPath = GetProjectPath(index: 5);
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        var expected = EvaluateProject(simpleProjectPath);
        AssertProjects(expected, actual);
    }

    [Fact]
    public void AnalyzeProjectWithCustomPropsReferences() {
        var simpleProjectPath = GetProjectPath(index: 6);
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        var expected = EvaluateProject(simpleProjectPath);
        AssertProjects(expected, actual);
    }

    [Fact]
    public void AnalyzeProjectWithDirectoryPropsReferences() {
        var simpleProjectPath = GetProjectPath(index: 7);
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        var expected = EvaluateProject(simpleProjectPath);
        AssertProjects(expected, actual);
    }
}
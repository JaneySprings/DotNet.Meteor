using Xunit;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Tests;

public class WorkspaceAnalysisTests: TestFixture {

    [Fact]
    public void AnalyzeSimpleProject() {
        var projectPath = CreateMockProject(@"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <PropertyGroup>
                <OutputType>Exe</OutputType>
                <UseMaui>true</UseMaui>
                <TargetFramework>net7.0-android</TargetFramework>
            </PropertyGroup>
        </Project>
        ");
        var actual = WorkspaceAnalyzer.AnalyzeProject(projectPath);

        Assert.Equal(ProjectName, actual.Name);
        Assert.Equal(projectPath, actual.Path);
        Assert.Equal("net7.0-android", string.Join(',', actual.Frameworks));
        DeleteMockData();
    }

    [Fact]
    public void AnalyzeProjectWithMultipleFrameworks() {
        var projectPath = CreateMockProject(@"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <PropertyGroup>
                <OutputType>Exe</OutputType>
                <UseMaui>true</UseMaui>
                <TargetFrameworks>net7.0-android;net6.0-ios;net7.0-maccatalyst;</TargetFrameworks>
                <!-- 
                    <TargetFrameworks>$(TargetFrameworks);net7.0-tizen</TargetFrameworks>  
                -->
            </PropertyGroup>
        </Project>
        ");
        var actual = WorkspaceAnalyzer.AnalyzeProject(projectPath);
        Assertion.CollectionsAreEqual(actual.Frameworks, new List<string>() {
            "net7.0-android",
            "net6.0-ios",
            "net7.0-maccatalyst",
        });
        DeleteMockData();
    }

    [Fact]
    public void AnalyzeProjectWithFrameworkReference() {
        var simpleProjectPath = CreateMockProject(@"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <PropertyGroup>
                <Android>net7.0-android</Android>
                <Apple>net7.0-maccatalyst;net7.0-ios</Apple>
                <OutputType>Exe</OutputType>
                <UseMaui>true</UseMaui>
                <TargetFrameworks>$(Android);$(Apple);net7.0-tizen</TargetFrameworks>
            </PropertyGroup>
        </Project>
        ");
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        Assertion.CollectionsAreEqual(actual.Frameworks, new List<string>() {
            "net7.0-android",
            "net7.0-maccatalyst",
            "net7.0-ios",
            "net7.0-tizen",
        });
        DeleteMockData();
    }

    [Fact]
    public void AnalyzeProjectWithFrameworkCondition() {
        var simpleProjectPath = CreateMockProject(@"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <PropertyGroup>
                <IsWindows>true</IsWindows>
                <OutputType>Exe</OutputType>
                <UseMaui>true</UseMaui>
                <TargetFrameworks>net7.0-maccatalyst</TargetFrameworks>
                <TargetFrameworks Condition=""$(IsWindows) == 'true'"">$(TargetFrameworks);net7.0-windows10.0.19041.0</TargetFrameworks>
                <!-- <TargetFrameworks>$(TargetFrameworks);net7.0-tizen</TargetFrameworks> -->
            </PropertyGroup>
        </Project>
        ");
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        Assertion.CollectionsAreEqual(actual.Frameworks, new List<string>() {
            "net7.0-maccatalyst",
            "net7.0-windows10.0.19041.0",
        });
        DeleteMockData();
    }

    [Fact]
    public void AnalyzeProjectWithDirectoryProps() {
        CreateCommonProps(".", @"
        <Project>
            <PropertyGroup>
                <TargetFrameworks>net7.0-maccatalyst</TargetFrameworks>
            </PropertyGroup>
        </Project>
        ");
        CreateCustomProps("Build.props", @"
        <Project>
            <PropertyGroup>
                <TargetFrameworks>net7.0-ios</TargetFrameworks>
            </PropertyGroup>
        </Project>
        ");
        var simpleProjectPath = CreateMockProject(@"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <PropertyGroup>
                <OutputType>Exe</OutputType>
                <UseMaui>true</UseMaui>
            </PropertyGroup>
        </Project>
        ");
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        Assertion.CollectionsAreEqual(actual.Frameworks, new List<string>() {
            "net7.0-maccatalyst"
        });
        DeleteMockData();
    }

    [Fact]
    public void AnalyzeProjectWithCustomPropsReferences() {
        var propsReference = CreateCustomProps("Build.props", @"
        <Project>
            <PropertyGroup>
                <TargetFrameworks>net7.0-ios</TargetFrameworks>
            </PropertyGroup>
        </Project>
        ");
        var simpleProjectPath = CreateMockProject(@$"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <Import Project=""{propsReference}"" />
            <PropertyGroup>
                <OutputType>Exe</OutputType>
                <UseMaui>true</UseMaui>
            </PropertyGroup>
        </Project>
        ");
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        Assertion.CollectionsAreEqual(actual.Frameworks, new List<string>() {
            "net7.0-ios"
        });
        DeleteMockData();
    }

    [Fact]
    public void AnalyzeProjectWithDirectoryPropsReferences() {
        CreateCommonProps("..", @"
        <Project>
            <PropertyGroup>
                <TargetFrameworks>net7.0-maccatalyst</TargetFrameworks>
            </PropertyGroup>
        </Project>
        ");
        var simpleProjectPath = CreateMockProject(@"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <PropertyGroup>
                <OutputType>Exe</OutputType>
                <UseMaui>true</UseMaui>
            </PropertyGroup>
        </Project>
        ");
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        Assertion.CollectionsAreEqual(actual.Frameworks, new List<string>() {
            "net7.0-maccatalyst"
        });
        DeleteMockData();
    }

    [Fact]
    public void OnlyExecutableProjectTest() {
        var simpleProjectPath = CreateMockProject(@"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <PropertyGroup>
                <OutputType>Library</OutputType>
                <UseMaui>true</UseMaui>
            </PropertyGroup>
        </Project>
        ");
        var callbackInvokeCount = 0;
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath, message => {
            Assert.Contains("executable", message);
            callbackInvokeCount++;
        });
        Assert.Null(actual);
        Assert.Equal(1, callbackInvokeCount);
        DeleteMockData();
    }
}
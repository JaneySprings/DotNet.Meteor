using DotNet.Meteor.Workspace;
using NUnit.Framework;

namespace DotNet.Meteor.Common.Tests;

public class WorkspaceAnalysisTests: TestFixture {

    [Test]
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

        Assert.Multiple(() => {
            Assert.That(actual, Is.Not.Null);
            Assert.That(ProjectName, Is.EqualTo(actual!.Name));
            Assert.That(projectPath, Is.EqualTo(actual.Path));
            Assert.That(string.Join(',', actual.Frameworks), Is.EqualTo("net7.0-android"));
            Assert.That(actual.Configurations.Count(), Is.EqualTo(2));
            Assert.That(string.Join(',', actual.Configurations), Is.EqualTo("Debug,Release"));
        });
    }
    [Test]
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
        Assert.That(actual, Is.Not.Null);
        CollectionsAreEqual(actual.Frameworks, new List<string>() {
            "net7.0-android",
            "net6.0-ios",
            "net7.0-maccatalyst",
        });
    }
    [Test]
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
        Assert.That(actual, Is.Not.Null);
        CollectionsAreEqual(actual.Frameworks, new List<string>() {
            "net7.0-android",
            "net7.0-maccatalyst",
            "net7.0-ios",
            "net7.0-tizen",
        });
    }
    [Test]
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
        Assert.That(actual, Is.Not.Null);
        CollectionsAreEqual(actual.Frameworks, new List<string>() {
            "net7.0-maccatalyst",
            "net7.0-windows10.0.19041.0",
        });
    }
    [Test]
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
        Assert.That(actual, Is.Not.Null);
        CollectionsAreEqual(actual.Frameworks, new List<string>() {
            "net7.0-maccatalyst"
        });
    }
    [Test]
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
        Assert.That(actual, Is.Not.Null);
        CollectionsAreEqual(actual.Frameworks, new List<string>() {
            "net7.0-ios"
        });
    }
    [Test]
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
        Assert.That(actual, Is.Not.Null);
        CollectionsAreEqual(actual.Frameworks, new List<string>() {
            "net7.0-maccatalyst"
        });
    }
    [Test]
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
            Assert.That(message, Does.Contain("executable"));
            callbackInvokeCount++;
        });
        Assert.Multiple(() => {
            Assert.That(actual, Is.Null);
            Assert.That(callbackInvokeCount, Is.EqualTo(1));
        });
    }
    [Test]
    public void IncorrectWorkspacePathTest() {
        var simpleProjectPath = CreateMockProject(@"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <PropertyGroup>
                <TargetFrameworks>net7.0-maccatalyst</TargetFrameworks>
                <OutputType>Exe</OutputType>
            </PropertyGroup>
        </Project>
        ");
        var callbackInvokeCount = 0;
        var directoryPath = Path.GetDirectoryName(simpleProjectPath)!;
        var actual = WorkspaceAnalyzer.AnalyzeWorkspace(Path.Combine(directoryPath, "MissingFolder"), message => {
            Assert.That(message, Does.StartWith("Could not find"));
            callbackInvokeCount++;
        });

        Assert.Multiple(() => {
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.Empty);
            Assert.That(callbackInvokeCount, Is.EqualTo(1));
        });
    }
    [Test]
    public void AnalyzeProjectWithCustomPropsWithThisFileDirectoryReference() {
        var propsReference = CreateCustomProps("MyProps.props", @"
        <Project>
            <PropertyGroup>
                <TargetFrameworks>net8.0-ios</TargetFrameworks>
                <OutputType>Exe</OutputType>
                <UseMaui>true</UseMaui>
            </PropertyGroup>
        </Project>
        ");
        var simpleProjectPath = CreateMockProject(@$"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <Import Project=""$(MSBuildThisFileDirectory)../MyProps.props""/>
        </Project>
        ");
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        Assert.That(actual, Is.Not.Null);
        CollectionsAreEqual(actual.Frameworks, new List<string>() {
            "net8.0-ios"
        });
    }
    [Test]
    public void AnalyzeProjectWithCustomPropsWithThisFileDirectoryReference_BackSlash() {
        var propsReference = CreateCustomProps("MyProps.props", @"
        <Project>
            <PropertyGroup>
                <TargetFrameworks>net8.0-ios</TargetFrameworks>
                <OutputType>Exe</OutputType>
                <UseMaui>true</UseMaui>
            </PropertyGroup>
        </Project>
        ");
        var simpleProjectPath = CreateMockProject(@$"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <Import Project=""$(MSBuildThisFileDirectory)..\MyProps.props""/>
        </Project>
        ");
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        Assert.That(actual, Is.Not.Null);
        CollectionsAreEqual(actual.Frameworks, new List<string>() {
            "net8.0-ios"
        });
    }
    [Test]
    public void AnalyzeProjectWithCustomPropsRelativePath() {
        var propsReference = CreateCustomProps("MyProps.props", @"
        <Project>
            <PropertyGroup>
                <TargetFrameworks>net8.0-ios</TargetFrameworks>
                <OutputType>Exe</OutputType>
                <UseMaui>true</UseMaui>
            </PropertyGroup>
        </Project>
        ");
        var simpleProjectPath = CreateMockProject(@$"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <Import Project=""../MyProps.props""/>
        </Project>
        ");
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        Assert.That(actual, Is.Not.Null);
        CollectionsAreEqual(actual.Frameworks, new List<string>() {
            "net8.0-ios"
        });
    }
    [Test]
    public void AnalyzeProjectWithCustomPropsRelativePath_BackSlash() {
        var propsReference = CreateCustomProps("MyProps.props", @"
        <Project>
            <PropertyGroup>
                <TargetFrameworks>net8.0-ios</TargetFrameworks>
                <OutputType>Exe</OutputType>
                <UseMaui>true</UseMaui>
            </PropertyGroup>
        </Project>
        ");
        var simpleProjectPath = CreateMockProject(@$"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <Import Project=""..\MyProps.props""/>
        </Project>
        ");
        var actual = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        Assert.That(actual, Is.Not.Null);
        CollectionsAreEqual(actual.Frameworks, new List<string>() {
            "net8.0-ios"
        });
    }
    [Test]
    public void CustomConfigurationsTest() {
        var projectPath = CreateMockProject(@"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <PropertyGroup>
                <OutputType>Exe</OutputType>
                <UseMaui>true</UseMaui>
                <TargetFramework>net7.0-android</TargetFramework>
                <Configurations>Debug;Release;Custom</Configurations>
            </PropertyGroup>
        </Project>
        ");
        var actual = WorkspaceAnalyzer.AnalyzeProject(projectPath);

        Assert.Multiple(() => {
            Assert.That(actual, Is.Not.Null);
            Assert.That(ProjectName, Is.EqualTo(actual!.Name));
            Assert.That(projectPath, Is.EqualTo(actual.Path));
            Assert.That(actual.Configurations.Count(), Is.EqualTo(3));
            Assert.That(string.Join(',', actual.Configurations), Is.EqualTo("Custom,Debug,Release"));
        });
    }
    [Test]
    public void CustomConfigurations2Test() {
        var projectPath = CreateMockProject(@"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <PropertyGroup>
                <OutputType>Exe</OutputType>
                <UseMaui>true</UseMaui>
                <TargetFramework>net7.0-android</TargetFramework>
                <Configurations>Custom</Configurations>
            </PropertyGroup>
        </Project>
        ");
        var actual = WorkspaceAnalyzer.AnalyzeProject(projectPath);

        Assert.Multiple(() => {
            Assert.That(actual, Is.Not.Null);
            Assert.That(ProjectName, Is.EqualTo(actual!.Name));
            Assert.That(projectPath, Is.EqualTo(actual.Path));
            Assert.That(actual.Configurations.Count(), Is.EqualTo(3));
            Assert.That(string.Join(',', actual.Configurations), Is.EqualTo("Custom,Debug,Release"));
        });
    }
    [Test]
    public void CustomConfigurations3Test() {
        var projectPath = CreateMockProject(@"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <PropertyGroup>
                <OutputType>Exe</OutputType>
                <UseMaui>true</UseMaui>
                <TargetFramework>net7.0-android</TargetFramework>
                <Configurations></Configurations>
            </PropertyGroup>
        </Project>
        ");
        var actual = WorkspaceAnalyzer.AnalyzeProject(projectPath);

        Assert.Multiple(() => {
            Assert.That(actual, Is.Not.Null);
            Assert.That(ProjectName, Is.EqualTo(actual!.Name));
            Assert.That(projectPath, Is.EqualTo(actual.Path));
            Assert.That(actual.Configurations.Count(), Is.EqualTo(2));
            Assert.That(string.Join(',', actual.Configurations), Is.EqualTo("Debug,Release"));
        });
    }
    [Test]
    public void CustomConfigurations4Test() {
        var propsReference = CreateCustomProps("MyProps.props", @"
        <Project>
            <PropertyGroup>
                <TargetFrameworks>net8.0-ios</TargetFrameworks>
                <OutputType>Exe</OutputType>
                <UseMaui>true</UseMaui>
                <Configurations>Custom</Configurations>
            </PropertyGroup>
        </Project>
        ");
        var projectPath = CreateMockProject(@$"
        <Project Sdk=""Microsoft.NET.Sdk"">
            <Import Project=""..\MyProps.props""/>
        </Project>
        ");
        var actual = WorkspaceAnalyzer.AnalyzeProject(projectPath);

        Assert.Multiple(() => {
            Assert.That(actual, Is.Not.Null);
            Assert.That(ProjectName, Is.EqualTo(actual!.Name));
            Assert.That(projectPath, Is.EqualTo(actual.Path));
            Assert.That(actual.Configurations.Count(), Is.EqualTo(3));
            Assert.That(string.Join(',', actual.Configurations), Is.EqualTo("Custom,Debug,Release"));
        });
    }

    [TearDown]
    public void TearDown() {
        Directory.Delete(MockDataDirectory, true);
    }
}
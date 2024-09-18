using Xunit;
using DotNet.Meteor.Common;
using DotNet.Meteor.Common.Extensions;

namespace DotNet.Meteor.Tests;

public class OtherTests : TestFixture {

    [Fact]
    public void AndroidSdkDirectoryTests() {
        var sdkLocation = AndroidSdk.SdkLocation();
        Assert.NotNull(sdkLocation);
        Assert.True(Directory.Exists(sdkLocation));
    }

    [Fact]
    public void HomeDirectoryValidationTest() {
        var homeDirectory = RuntimeSystem.HomeDirectory;
        if (RuntimeSystem.IsWindows)
            Assert.StartsWith("C:\\Users", homeDirectory);
        else if (RuntimeSystem.IsMacOS)
            Assert.StartsWith("/Users", homeDirectory);
        else
            Assert.StartsWith("/home", homeDirectory);
    }

    [Fact]
    public void ProgramFilesDirectoryValidationTest() {
        if (!RuntimeSystem.IsWindows)
            return;

        var homeDirectory = RuntimeSystem.ProgramX86Directory;
        Assert.StartsWith("C:\\Program", homeDirectory);
    }

    [Fact]
    public void DiagnosticToolsHasSameTargetFrameworkVersionTest() {
        var cwd = AppDomain.CurrentDomain.BaseDirectory;
        var dotnetTraceProjectPath = Path.Combine(cwd, "..", "..", "..", "DotNet.Diagnostics", "src", "Tools", "dotnet-trace", "dotnet-trace.csproj");
        var dotnetGCDumpProjectPath = Path.Combine(cwd, "..", "..", "..", "DotNet.Diagnostics", "src", "Tools", "dotnet-gcdump", "dotnet-gcdump.csproj");
        var dotnetDSRouterProjectPath = Path.Combine(cwd, "..", "..", "..", "DotNet.Diagnostics", "src", "Tools", "dotnet-dsrouter", "dotnet-dsrouter.csproj");
        var meteorPropsPath = Path.Combine(cwd, "..", "..", "..", "Common.Build.props");

        Assert.True(File.Exists(dotnetTraceProjectPath), $"File not found: {Path.GetFullPath(dotnetTraceProjectPath)}");
        Assert.True(File.Exists(dotnetGCDumpProjectPath), $"File not found: {Path.GetFullPath(dotnetGCDumpProjectPath)}");
        Assert.True(File.Exists(dotnetDSRouterProjectPath), $"File not found: {Path.GetFullPath(dotnetDSRouterProjectPath)}");
        Assert.True(File.Exists(meteorPropsPath), $"File not found: {Path.GetFullPath(meteorPropsPath)}");

        var dotnetTraceProject = new Project(dotnetTraceProjectPath);
        var dotnetGCDumpProject = new Project(dotnetGCDumpProjectPath);
        var dotnetDSRouterProject = new Project(dotnetDSRouterProjectPath);
        var meteorProject = new Project(meteorPropsPath);
        var expectedTargetFramework = meteorProject.EvaluateProperty("TargetFramework", "error1");

        Assert.Multiple(() => {
            Assert.Equal(expectedTargetFramework, dotnetTraceProject.EvaluateProperty("TargetFramework", "error2"));
            Assert.Equal(expectedTargetFramework, dotnetGCDumpProject.EvaluateProperty("TargetFramework", "error3"));
            Assert.Equal(expectedTargetFramework, dotnetDSRouterProject.EvaluateProperty("TargetFramework", "error4"));
        });
    }
}
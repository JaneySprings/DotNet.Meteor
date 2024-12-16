using DotNet.Meteor.Common.Extensions;
using DotNet.Meteor.Common.Android;
using NUnit.Framework;

namespace DotNet.Meteor.Common.Tests;

public class OtherTests : TestFixture {

    [Test]
    public void AndroidSdkDirectoryTests() {
        var sdkLocation = AndroidSdkLocator.SdkLocation();
        Assert.Multiple(() => {
            Assert.That(sdkLocation, Is.Not.Null.Or.Empty);
            Assert.That(Directory.Exists(sdkLocation));
        });
    }
    [Test]
    public void HomeDirectoryValidationTest() {
        var homeDirectory = RuntimeSystem.HomeDirectory;
        if (RuntimeSystem.IsWindows)
            Assert.That(homeDirectory, Does.StartWith("C:\\Users"));
        else if (RuntimeSystem.IsMacOS)
            Assert.That(homeDirectory, Does.StartWith("/Users"));
        else
            Assert.That(homeDirectory, Does.StartWith("/home"));
    }
    [Test]
    public void ProgramFilesDirectoryValidationTest() {
        if (!RuntimeSystem.IsWindows)
            return;

        var programsDirectory = RuntimeSystem.ProgramX86Directory;
        Assert.That(programsDirectory, Does.StartWith("C:\\Program"));
    }
    [Test]
    public void DiagnosticToolsHasSameTargetFrameworkVersionTest() {
        var cwd = AppDomain.CurrentDomain.BaseDirectory;
        var dotnetTraceProjectPath = Path.Combine(cwd, "..", "..", "..", "DotNet.Diagnostics", "src", "Tools", "dotnet-trace", "dotnet-trace.csproj");
        var dotnetGCDumpProjectPath = Path.Combine(cwd, "..", "..", "..", "DotNet.Diagnostics", "src", "Tools", "dotnet-gcdump", "dotnet-gcdump.csproj");
        var dotnetDSRouterProjectPath = Path.Combine(cwd, "..", "..", "..", "DotNet.Diagnostics", "src", "Tools", "dotnet-dsrouter", "dotnet-dsrouter.csproj");
        var meteorPropsPath = Path.Combine(cwd, "..", "..", "..", "Common.Build.props");

        Assert.Multiple(() => {
            Assert.That(File.Exists(dotnetTraceProjectPath), $"File not found: {Path.GetFullPath(dotnetTraceProjectPath)}");
            Assert.That(File.Exists(dotnetGCDumpProjectPath), $"File not found: {Path.GetFullPath(dotnetGCDumpProjectPath)}");
            Assert.That(File.Exists(dotnetDSRouterProjectPath), $"File not found: {Path.GetFullPath(dotnetDSRouterProjectPath)}");
            Assert.That(File.Exists(meteorPropsPath), $"File not found: {Path.GetFullPath(meteorPropsPath)}");
        });

        var dotnetTraceProject = new Project(dotnetTraceProjectPath);
        var dotnetGCDumpProject = new Project(dotnetGCDumpProjectPath);
        var dotnetDSRouterProject = new Project(dotnetDSRouterProjectPath);
        var meteorProject = new Project(meteorPropsPath);
        var expectedTargetFramework = meteorProject.EvaluateProperty("TargetFramework", "error1");

        Assert.Multiple(() => {
            Assert.That(dotnetTraceProject.EvaluateProperty("TargetFramework", "error2"), Is.EqualTo(expectedTargetFramework));
            Assert.That(dotnetGCDumpProject.EvaluateProperty("TargetFramework", "error3"), Is.EqualTo(expectedTargetFramework));
            Assert.That(dotnetDSRouterProject.EvaluateProperty("TargetFramework", "error4"), Is.EqualTo(expectedTargetFramework));
        });
    }
    [Test]
    public void ToolingDefaultsTest() {
        Assert.That(Common.Apple.MonoLauncher.UseDeviceCtl, Is.False, "UseDeviceCtl should be false: https://github.com/xamarin/xamarin-macios/issues/21664");
    }
}
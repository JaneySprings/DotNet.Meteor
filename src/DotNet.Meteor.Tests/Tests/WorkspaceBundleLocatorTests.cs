using Xunit;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Tests;

public class WorkspaceBundleLocatorTests: TestFixture {

    [Theory]
    [InlineData("Debug", "net7.0-android", "com.debug.net7-Signed.apk")]
    [InlineData("Debug", "net6.0-android", "com.debug.net6-Signed.apk")]
    [InlineData("Release", "net7.0-android", "com.release.net7-Signed.apk")]
    [InlineData("Release", "net6.0-android", "com.release.net6-Signed.apk")]
    public void AndroidPackageLocationTests(string configuration, string framework, string bundleName) {
        var device = new DeviceData { Platform = Platforms.Android };
        var simpleProjectPath = GetProjectPath(index: 1);

        var aProject = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        var mProject = EvaluateProject(simpleProjectPath, new Dictionary<string, string> {
            { "Configuration", configuration },
            { "TargetFramework", framework }
        });

        Assert.NotNull(aProject);
        var actualPath = aProject.GetOutputAssembly(configuration, framework, device);
        var expectedPath = Path.Combine(mProject.DirectoryPath, mProject.GetPropertyValue("OutputPath"), bundleName);
        Assert.Equal(expectedPath.ToPlatformPath(), actualPath);
        UnloadProject(mProject);
    }

    [Theory]
    [InlineData("Debug", "debug_x64.app")]
    [InlineData("Release", "release_x64.app")]
    public void iOSSimylatorBundleLocationTests(string configuration, string bundleName) {
        if (!RuntimeSystem.IsMacOS)
            return;

        var device = new DeviceData { Platform = Platforms.iOS, RuntimeId = Runtimes.iOSimulatorX64 };
        var simpleProjectPath = GetProjectPath(index: 1);
        var framework = "net7.0-ios";

        var aProject = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        var mProject = EvaluateProject(simpleProjectPath, new Dictionary<string, string> {
            { "Configuration", configuration },
            { "TargetFramework", framework },
            { "RuntimeIdentifier", Runtimes.iOSimulatorX64 }
        });

        Assert.NotNull(aProject);
        var actualPath = aProject.GetOutputAssembly(configuration, framework, device);
        var expectedPath = Path.Combine(mProject.DirectoryPath, mProject.GetPropertyValue("OutputPath"), bundleName);
        Assert.Equal(expectedPath.ToPlatformPath(), actualPath);
        UnloadProject(mProject);
    }

    [Theory]
    [InlineData("Debug", "debug_arm.app")]
    [InlineData("Release", "release_arm.app")]
    public void iOSArmBundleLocationTests(string configuration, string bundleName) {
        if (!RuntimeSystem.IsMacOS)
            return;

        var device = new DeviceData { Platform = Platforms.iOS, RuntimeId = Runtimes.iOSArm64 };
        var simpleProjectPath = GetProjectPath(index: 1);
        var framework = "net7.0-ios";

        var aProject = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        var mProject = EvaluateProject(simpleProjectPath, new Dictionary<string, string> {
            { "Configuration", configuration },
            { "TargetFramework", framework },
            { "RuntimeIdentifier", Runtimes.iOSArm64 }
        });

        Assert.NotNull(aProject);
        var actualPath = aProject.GetOutputAssembly(configuration, framework, device);
        var expectedPath = Path.Combine(mProject.DirectoryPath, mProject.GetPropertyValue("OutputPath"), bundleName);
        Assert.Equal(expectedPath.ToPlatformPath(), actualPath);
        UnloadProject(mProject);
    }

    [Theory]
    [InlineData("Debug", "mac_debug_x64.app")]
    [InlineData("Release", "mac_release_x64.app")]
    public void MacX64BundleLocationTests(string configuration, string bundleName) {
        if (!RuntimeSystem.IsMacOS)
            return;

        var device = new DeviceData { Platform = Platforms.MacCatalyst, RuntimeId = Runtimes.MacX64 };
        var simpleProjectPath = GetProjectPath(index: 1);
        var framework = "net7.0-maccatalyst";

        var aProject = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        var mProject = EvaluateProject(simpleProjectPath, new Dictionary<string, string> {
            { "Configuration", configuration },
            { "TargetFramework", framework },
            { "RuntimeIdentifier", Runtimes.MacX64 }
        });

        Assert.NotNull(aProject);
        var actualPath = aProject.GetOutputAssembly(configuration, framework, device);
        var expectedPath = Path.Combine(mProject.DirectoryPath, mProject.GetPropertyValue("OutputPath"), bundleName);
        Assert.Equal(expectedPath.ToPlatformPath(), actualPath);
        UnloadProject(mProject);
    }

    [Theory]
    [InlineData("Debug", "mac_debug_arm.app")]
    [InlineData("Release", "mac_release_arm.app")]
    public void MacArmBundleLocationTests(string configuration, string bundleName) {
        if (RuntimeSystem.IsWindows)
            return;

        var device = new DeviceData { Platform = Platforms.MacCatalyst, RuntimeId = Runtimes.MacArm64 };
        var simpleProjectPath = GetProjectPath(index: 1);
        var framework = "net7.0-maccatalyst";

        var aProject = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        var mProject = EvaluateProject(simpleProjectPath, new Dictionary<string, string> {
            { "Configuration", configuration },
            { "TargetFramework", framework },
            { "RuntimeIdentifier", Runtimes.MacArm64 }
        });

        Assert.NotNull(aProject);
        var actualPath = aProject.GetOutputAssembly(configuration, framework, device);
        var expectedPath = Path.Combine(mProject.DirectoryPath, mProject.GetPropertyValue("OutputPath"), bundleName);
        Assert.Equal(expectedPath.ToPlatformPath(), actualPath);
        UnloadProject(mProject);
    }

    [Theory]
    [InlineData("Debug", "TestApp1.exe")]
    [InlineData("Release", "TestApp1.exe")]
    public void WindowsExecutableLocationTests(string configuration, string bundleName) {
        if (!RuntimeSystem.IsWindows)
            return;

        var device = new DeviceData { Platform = Platforms.Windows };
        var simpleProjectPath = GetProjectPath(index: 1);
        var framework = "net7.0-windows10.0.19041.0";

        var aProject = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);
        var mProject = EvaluateProject(simpleProjectPath, new Dictionary<string, string> {
            { "Configuration", configuration },
            { "TargetFramework", framework },
        });

        Assert.NotNull(aProject);
        var actualPath = aProject.GetOutputAssembly(configuration, framework, device);
        var expectedPath = Path.Combine(mProject.DirectoryPath, mProject.GetPropertyValue("OutputPath"), bundleName);
        Assert.Equal(expectedPath.ToPlatformPath(), actualPath);
        UnloadProject(mProject);
    }

    [Theory]
    [InlineData("Debug", "net7.0-android")]
    [InlineData("Release", "net7.0-android")]
    public void EmptyLocationTests(string configuration, string framework) {
        var device = new DeviceData { Platform = Platforms.Android };
        var simpleProjectPath = GetProjectPath(index: 2);
        var aProject = WorkspaceAnalyzer.AnalyzeProject(simpleProjectPath);

        Assert.NotNull(aProject);
        Assert.Throws<DirectoryNotFoundException>(() => aProject.GetOutputAssembly(configuration, framework, device));
    }
}
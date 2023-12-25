using Xunit;
using DotNet.Meteor.Shared;
using DotNet.Meteor.Workspace;

namespace DotNet.Meteor.Tests;

public class WorkspaceBundleLocatorTests: TestFixture {
    private const string SimpleProject = @"
    <Project Sdk=""Microsoft.NET.Sdk"">
        <PropertyGroup>
            <OutputType>Exe</OutputType>
            <UseMaui>true</UseMaui>
            <TargetFramework>net7.0-ios</TargetFramework>
        </PropertyGroup>
    </Project>
    ";

    [Theory]
    [InlineData("Debug", "net7.0-android", "com.debug-Signed.apk", DeviceService.Android)]
    [InlineData("Debug", "net6.0-android", "com.debug-Signed.apk", DeviceService.Android)]
    [InlineData("Release", "net7.0-android", "com.release-Signed.apk", DeviceService.Android)]
    [InlineData("Release", "net6.0-android", "com.release-Signed.apk", DeviceService.Android)]

    [InlineData("Debug", "net7.0-ios", "debug_x64.app", DeviceService.AppleSimulatorX64)]
    [InlineData("Debug", "net6.0-ios", "debug_x64.app", DeviceService.AppleSimulatorX64)]
    [InlineData("Release", "net7.0-ios", "release_x64.app", DeviceService.AppleSimulatorX64)]
    [InlineData("Release", "net6.0-ios", "release_x64.app", DeviceService.AppleSimulatorX64)]
    [InlineData("Debug", "net7.0-ios", "debug_arm64.app", DeviceService.AppleArm64)]
    [InlineData("Debug", "net6.0-ios", "debug_arm64.app", DeviceService.AppleArm64)]
    [InlineData("Release",  "net7.0-ios", "release_arm64.app", DeviceService.AppleArm64)]
    [InlineData("Release",  "net6.0-ios", "release_arm64.app", DeviceService.AppleArm64)]

    [InlineData("Debug", "net7.0-maccatalyst", "debug_x64.app", DeviceService.MacX64)]
    [InlineData("Debug", "net6.0-maccatalyst", "debug_x64.app", DeviceService.MacX64)]
    [InlineData("Release", "net7.0-maccatalyst", "release_x64.app", DeviceService.MacX64)]
    [InlineData("Release", "net6.0-maccatalyst", "release_x64.app", DeviceService.MacX64)]
    [InlineData("Debug", "net7.0-maccatalyst", "debug_arm64.app", DeviceService.MacArm64)]
    [InlineData("Debug", "net6.0-maccatalyst", "debug_arm64.app", DeviceService.MacArm64)]
    [InlineData("Release",  "net7.0-maccatalyst", "release_arm64.app", DeviceService.MacArm64)]
    [InlineData("Release",  "net6.0-maccatalyst", "release_arm64.app", DeviceService.MacArm64)]

    [InlineData("Debug", "net7.0-windows10.0.19041.0", "TestApp.exe", DeviceService.Windows10, true)]
    [InlineData("Release", "net7.0-windows10.0.19041.0", "TestApp.exe", DeviceService.Windows10, true)]
    public void AndroidPackageLocationTests(string configuration, string framework, string bundleName, string deviceId, bool includeWinX64Dir = false) {
        var device = DeviceService.GetDevice(deviceId)!;
        var projectPath = CreateMockProject(SimpleProject);
        var project = WorkspaceAnalyzer.AnalyzeProject(projectPath);
        var expectedPath = device.IsIPhone || device.IsMacCatalyst
            ? CreateOutputBundle(configuration, framework, device.RuntimeId, bundleName)
            : CreateOutputAssembly(configuration, framework, device.RuntimeId, bundleName, includeWinX64Dir);
        var actualPath = project.FindOutputApplication(configuration, framework, device, message => throw new ArgumentException(message));

        Assert.Equal(expectedPath, actualPath);
        DeleteMockData();
    }


    [Theory]
    [InlineData("Debug", "net7.0-android", DeviceService.Android)]
    [InlineData("Release", "net6.0-android", DeviceService.Android)]

    [InlineData("Debug", "net7.0-ios", DeviceService.AppleSimulatorX64)]
    [InlineData("Release", "net6.0-ios", DeviceService.AppleSimulatorX64)]
    [InlineData("Debug", "net7.0-ios", DeviceService.AppleArm64)]
    [InlineData("Release",  "net6.0-ios", DeviceService.AppleArm64)]

    [InlineData("Debug", "net7.0-maccatalyst", DeviceService.MacX64)]
    [InlineData("Release", "net6.0-maccatalyst", DeviceService.MacX64)]
    [InlineData("Debug", "net7.0-maccatalyst", DeviceService.MacArm64)]
    [InlineData("Release", "net6.0-maccatalyst", DeviceService.MacArm64)]

    [InlineData("Debug", "net7.0-windows10.0.19041.0", DeviceService.Windows10)]
    [InlineData("Release", "net7.0-windows10.0.19041.0", DeviceService.Windows10)]
    public void EmptyLocationTests(string configuration, string framework, string deviceId) {
        var device = DeviceService.GetDevice(deviceId)!;
        var projectPath = CreateMockProject(SimpleProject);
        var project = WorkspaceAnalyzer.AnalyzeProject(projectPath);

        Assert.Throws<ArgumentException>(() => project.FindOutputApplication(configuration, framework, device, message => throw new ArgumentException(message)));
        DeleteMockData();
    }

    [Fact]
    public void MultipleAndroidOutputPathsTests() {
        var projectPath = CreateMockProject(SimpleProject);
        var project = WorkspaceAnalyzer.AnalyzeProject(projectPath);
        var device = DeviceService.GetDevice(DeviceService.Android)!;
        var configuration = "Debug";
        var framework = "net8.0-android";
        var throwMessage = string.Empty;
    
        CreateOutputAssembly(configuration, framework, device.RuntimeId, "com.debug-Signed.apk", false);
        CreateOutputAssembly(configuration, framework, device.RuntimeId, "com.debug2-Signed.apk", false);
    
        Assert.Throws<ArgumentException>(() => project.FindOutputApplication(configuration, framework, device, message => {
            throwMessage = message;
            throw new ArgumentException(message);
        }));
        Assert.Contains("Found more than one", throwMessage);
        DeleteMockData();
    }

    [Fact]
    public void MultipleAppleOutputPathsTests() {
        var projectPath = CreateMockProject(SimpleProject);
        var project = WorkspaceAnalyzer.AnalyzeProject(projectPath);
        var device = DeviceService.GetDevice(DeviceService.AppleSimulatorX64)!;
        var configuration = "Debug";
        var framework = "net8.0-ios";
        var throwMessage = string.Empty;
    
        CreateOutputBundle(configuration, framework, device.RuntimeId, "com.companyname.debug.app");
        CreateOutputBundle(configuration, framework, device.RuntimeId, "com.companyname.debug2.app");
    
        Assert.Throws<ArgumentException>(() => project.FindOutputApplication(configuration, framework, device, message => {
            throwMessage = message;
            throw new ArgumentException(message);
        }));
        Assert.Contains("Found more than one", throwMessage);
        DeleteMockData();
    }

    [Fact]
    public void MultipleArchOutputPathsTests() {
        var projectPath = CreateMockProject(SimpleProject);
        var project = WorkspaceAnalyzer.AnalyzeProject(projectPath);
        var device1 = DeviceService.GetDevice(DeviceService.AppleSimulatorX64)!;
        var device2 = DeviceService.GetDevice(DeviceService.AppleArm64)!;
        var configuration = "Debug";
        var framework = "net8.0-ios";
    
        CreateOutputBundle(configuration, framework, device1.RuntimeId, "com.companyname.debug.app");
        CreateOutputBundle(configuration, framework, device2.RuntimeId, "com.companyname.debug.app");
        project.FindOutputApplication(configuration, framework, device1, message => throw new ArgumentException(message));
        project.FindOutputApplication(configuration, framework, device2, message => throw new ArgumentException(message));
        DeleteMockData();
    }
}
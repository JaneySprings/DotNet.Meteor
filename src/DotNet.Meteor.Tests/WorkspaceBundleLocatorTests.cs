using Xunit;
using DotNet.Meteor.Shared;

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

    [InlineData("Debug", "net7.0-windows10.0.19041.0", "TestApp.exe", DeviceService.Windows10)]
    [InlineData("Release", "net7.0-windows10.0.19041.0", "TestApp.exe", DeviceService.Windows10)]
    public void AndroidPackageLocationTests(string configuration, string framework, string bundleName, string deviceId) {
        var device = DeviceService.GetDevice(deviceId)!;
        var projectPath = CreateMockProject(SimpleProject);
        var project = WorkspaceAnalyzer.AnalyzeProject(projectPath);
        var expectedPath = device.IsIPhone || device.IsMacCatalyst
            ? CreateOutputBundle(configuration, framework, device.RuntimeId, bundleName)
            : CreateOutputAssembly(configuration, framework, device.RuntimeId, bundleName);
        var actualPath = project.GetOutputAssembly(configuration, framework, device);

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

        Assert.Throws<DirectoryNotFoundException>(() => project.GetOutputAssembly(configuration, framework, device));
        DeleteMockData();
    }
}
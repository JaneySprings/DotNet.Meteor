using Xunit;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;
using DotNet.Meteor.Windows;
using DotNet.Meteor.Android;
using DotNet.Meteor.Apple;

namespace DotNet.Meteor.Tests;

public class DeviceProvidingTests: TestFixture {

    [Fact(Skip = "Requires Android SDK with system-images installed")]
    public void AndroidVirtualDeviceTest() {
        var avdTool = Android.PathUtils.AvdTool();
        var avdCreate = new ProcessRunner(avdTool, new ProcessArgumentBuilder()
            .Append("create", "avd")
            .Append("-n", "test")
            .Append("-k", "'system-images;android-31;google_apis;x86'")
            .Append("--force"))
            .WaitForExit();

        if (avdCreate.ExitCode != 0)
            throw new Exception(string.Join(Environment.NewLine, avdCreate.StandardError));

        var result = AndroidTool.VirtualDevices();
        Assert.NotNull(result);
    }

    [Fact]
    public void AndroidPhysicalDeviceTest() {
        var result = AndroidTool.PhysicalDevices();
        Assert.NotNull(result);
    }

    [Fact]
    public void AppleVirtualDeviceTest() {
        if (!RuntimeSystem.IsMacOS)
            return;
        var result = AppleTool.VirtualDevices();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ApplePhysicalDeviceTest() {
        if (!RuntimeSystem.IsMacOS)
            return;
        var result = AppleTool.PhysicalDevices();
        Assert.NotNull(result);
    }

    [Fact]
    public void AppleMacDeviceTest() {
        if (!RuntimeSystem.IsMacOS)
            return;
        var result = AppleTool.MacintoshDevice();
        Assert.NotNull(result);
    }

    [Fact]
    public void WindowsDeviceTest() {
        if (!RuntimeSystem.IsWindows)
            return;
        var result = WindowsTool.WindowsDevice();
        Assert.NotNull(result);
    }
}
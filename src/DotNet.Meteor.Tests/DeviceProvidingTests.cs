using DotNet.Meteor.Shared;
using DotNet.Meteor.Workspace.Android;
using DotNet.Meteor.Workspace.Apple;
using DotNet.Meteor.Workspace.Windows;
using Xunit;

namespace DotNet.Meteor.Tests;

public class DeviceProvidingTests: TestFixture {

    [Fact]
    public void AndroidPhysicalDeviceTest() {
        if (string.IsNullOrEmpty(AndroidSdk.SdkLocation()))
            return; // Skip test if sdk is not found

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
        var result = AppleTool.MacintoshDevices();

        if (SystemProfiler.IsArch64()) Assert.Equal(2, result.Count);
        else Assert.Single(result);
    }

    [Fact]
    public void WindowsDeviceTest() {
        if (!RuntimeSystem.IsWindows)
            return;
        var result = WindowsTool.WindowsDevice();
        Assert.NotNull(result);
    }
}
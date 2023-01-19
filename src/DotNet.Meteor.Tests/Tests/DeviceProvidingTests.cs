using Xunit;
using DotNet.Meteor.Shared;
using DotNet.Meteor.Windows;
using DotNet.Meteor.Android;
using DotNet.Meteor.Apple;

namespace DotNet.Meteor.Tests;

public class DeviceProvidingTests: TestFixture {

    [Fact]
    public void AndroidVirtualDeviceTest() {
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
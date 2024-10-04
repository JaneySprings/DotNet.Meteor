using DotNet.Meteor.Common;
using DotNet.Meteor.Common.Android;
using DotNet.Meteor.Common.Apple;
using DotNet.Meteor.Common.Windows;
using Xunit;

namespace DotNet.Meteor.Tests;

public class DeviceProvidingTests: TestFixture {

    [Fact]
    public void AndroidPhysicalDeviceTest() {
        // Hangs only on Azure Pipelines, i don't know why
        if (RuntimeSystem.IsWindows)
            return;

        try {
            var result = AndroidDeviceTool.PhysicalDevices();
            Assert.NotNull(result);
        } catch (FileNotFoundException e) {
            System.Diagnostics.Debug.WriteLine(e);
            return;
        }
    }

    [Fact]
    public void AppleVirtualDeviceTest() {
        if (!RuntimeSystem.IsMacOS)
            return;
        var result = AppleDeviceTool.VirtualDevices();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ApplePhysicalDeviceTest() {
        if (!RuntimeSystem.IsMacOS)
            return;
        var result = AppleDeviceTool.PhysicalDevices();
        Assert.NotNull(result);
    }

    [Fact]
    public void AppleMacDeviceTest() {
        if (!RuntimeSystem.IsMacOS)
            return;
        var result = AppleDeviceTool.MacintoshDevices();

        if (RuntimeSystem.IsAarch64) Assert.Equal(2, result.Count);
        else Assert.Single(result);
    }

    [Fact]
    public void WindowsDeviceTest() {
        if (!RuntimeSystem.IsWindows)
            return;
        var result = WindowsDeviceTool.WindowsDevice();
        Assert.NotNull(result);
    }
}
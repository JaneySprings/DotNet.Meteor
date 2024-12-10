using DotNet.Meteor.Common.Apple;
using DotNet.Meteor.Common.Windows;
using NUnit.Framework;

namespace DotNet.Meteor.Common.Tests;

public class DeviceProvidingTests: TestFixture {

    [Test]
    public void AppleVirtualDeviceTest() {
        if (!RuntimeSystem.IsMacOS)
            return;
        var result = AppleDeviceTool.VirtualDevices();
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);
    }
    [Test]
    public void ApplePhysicalDeviceTest() {
        if (!RuntimeSystem.IsMacOS)
            return;
        var result = AppleDeviceTool.PhysicalDevices();
        Assert.That(result, Is.Not.Null);
        // Can be empty if no physical devices are connected
    }
    [Test]
    public void AppleMacDeviceTest() {
        if (!RuntimeSystem.IsMacOS)
            return;
        var result = AppleDeviceTool.MacintoshDevices();

        if (RuntimeSystem.IsAarch64) 
            Assert.That(result, Has.Count.EqualTo(2));
        else 
            Assert.That(result, Has.Count.EqualTo(1));
    }
    [Test]
    public void WindowsDeviceTest() {
        if (!RuntimeSystem.IsWindows)
            return;
        var result = WindowsDeviceTool.WindowsDevice();
        Assert.That(result, Is.Not.Null);
    }
}
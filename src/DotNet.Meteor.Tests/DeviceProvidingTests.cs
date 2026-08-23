using DotNet.Meteor.Workspace;
using DotNet.Meteor.Workspace.Devices;
using NUnit.Framework;

namespace DotNet.Meteor.Tests;

public class DeviceProvidingTests : TestFixture {

    [Test]
    public void AppleVirtualDeviceTest() {
        if (!RuntimeInfo.IsMacOS)
            Assert.Ignore("MacOS only");
        var result = AppleDeviceTool.VirtualDevices();
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);
    }
    [Test]
    public void ApplePhysicalDeviceTest() {
        if (!RuntimeInfo.IsMacOS)
            Assert.Ignore("MacOS only");
        var result = AppleDeviceTool.PhysicalDevices();
        Assert.That(result, Is.Not.Null);
        // Can be empty if no physical devices are connected
    }
    [Test]
    public void AppleMacDeviceTest() {
        if (!RuntimeInfo.IsMacOS)
            Assert.Ignore("MacOS only");
        var result = AppleDeviceTool.MacintoshDevices();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].RuntimeId, Is.Null.Or.Empty);
    }
    [Test]
    public void WindowsDeviceTest() {
        if (!RuntimeInfo.IsWindows)
            Assert.Ignore("Windows only");
        var result = WindowsDeviceTool.WindowsDevice();
        Assert.That(result, Is.Not.Null);
    }
}
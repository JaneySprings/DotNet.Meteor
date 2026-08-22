using DotNet.Meteor.Common.Android;
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

        if (RuntimeSystem.IsAarch64) {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].RuntimeId, Is.Null.Or.Empty);
            Assert.That(result[1].RuntimeId, Is.EqualTo(Runtimes.MacX64));
        } else{
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].RuntimeId, Is.Null.Or.Empty);
        } 
    }
    [Test]
    public void AndroidDeviceSerialParsingTest() {
        var output = new[] {
            "List of devices attached",
            "* daemon not running; starting now at tcp:5037",
            "* daemon started successfully",
            "192.168.31.106:42805\tdevice product:zircon_global model:23090RA98G device:zircon transport_id:2",
            "adb-TQY9K0XXXXXXXXXXXX-XXXXXXXX device product:... model:Pixel 8 device:long name with spaces transport_id:3",
            "192.168.31.106:5555\toffline",
            "10.0.0.9:5555\tunauthorized",
            ""
        };
        var expected = new[] { "192.168.31.106:42805", "adb-TQY9K0XXXXXXXXXXXX-XXXXXXXX" };
        var result = AndroidDebugBridge.ParseDeviceSerials(output);
        CollectionsAreEqual(expected, result);
    }
    [Test]
    public void AndroidDeviceSerialParsingIgnoresNonDeviceStatesTest() {
        var output = new[] {
            "List of devices attached",
            "emulator-5554\tdevice",
            "1.2.3.4:5555\trecovery",
            "1.2.3.4:5556\tsideload",
            "1.2.3.4:5557\tconnecting",
            ""
        };
        var expected = new[] { "emulator-5554" };
        var result = AndroidDebugBridge.ParseDeviceSerials(output);
        CollectionsAreEqual(expected, result);
    }
    [Test]
    public void WindowsDeviceTest() {
        if (!RuntimeSystem.IsWindows)
            return;
        var result = WindowsDeviceTool.WindowsDevice();
        Assert.That(result, Is.Not.Null);
    }
}
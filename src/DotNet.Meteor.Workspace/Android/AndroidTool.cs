using DotNet.Meteor.Shared;
using DotNet.Meteor.Workspace.Utilities;

namespace DotNet.Meteor.Workspace.Android;

public static class AndroidTool {
    public static List<DeviceData> VirtualDevices() {
        var runningAvds = new Dictionary<string, string>();
        var avds = new List<DeviceData>();
        var avdHome = AndroidSdk.AvdLocation();

        foreach(var serial in DeviceBridge.Devices()) {
            if (!serial.StartsWith("emulator-"))
                continue;
            runningAvds.Add(DeviceBridge.EmuName(serial), serial);
        }

        if (Directory.Exists(avdHome)) {
            foreach (var file in Directory.GetFiles(avdHome, "*.ini")) {
                var ini = new IniFile(file);
                var name = Path.GetFileNameWithoutExtension(file);
                avds.Add(new DeviceData {
                    Name = name,
                    Serial = runningAvds.ContainsKey(name) ? runningAvds[name] : null,
                    Detail = Details.AndroidEmulator,
                    Platform = Platforms.Android,
                    OSVersion = ini.GetField("target") ?? "Unknown",
                    IsRunning = runningAvds.ContainsKey(name),
                    IsEmulator = true,
                    IsMobile = true
                });
                runningAvds.Remove(name);
                ini.Free();
            }
        }

        // Add all running AVDs that aren't in the AVD folder
        foreach (var avd in runningAvds) {
            avds.Add(new DeviceData {
                Name = avd.Key,
                Serial = avd.Value,
                Detail = Details.AndroidEmulator,
                Platform = Platforms.Android,
                OSVersion = $"android-{DeviceBridge.Shell(avd.Value, "getprop", "ro.build.version.sdk")}",
                IsRunning = true,
                IsEmulator = true,
                IsMobile = true
            });
        }

        return avds;
    }

    public static List<DeviceData> PhysicalDevices() {
        var runningDevices = DeviceBridge.Devices();
        var devices = new List<DeviceData>();

        foreach(var serial in runningDevices) {
            if (serial.StartsWith("emulator-"))
                continue;

            devices.Add(new DeviceData {
                Name = DeviceBridge.Shell(serial, "getprop", "ro.product.model"),
                OSVersion = $"android-{DeviceBridge.Shell(serial, "getprop", "ro.build.version.sdk")}",
                Platform = Platforms.Android,
                Detail = Details.AndroidDevice,
                IsEmulator = false,
                IsRunning = true,
                IsMobile = true,
                Serial = serial
            });
        }

        return devices;
    }
}
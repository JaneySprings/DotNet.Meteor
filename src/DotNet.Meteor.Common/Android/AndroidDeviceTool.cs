using DotNet.Meteor.Common.Utilities;

namespace DotNet.Meteor.Common.Android;

public static class AndroidDeviceTool {
    public static List<DeviceData> VirtualDevices() {
        var runningAvds = new Dictionary<string, string>();
        var avds = new List<DeviceData>();
        var avdHome = AndroidSdkLocator.AvdLocation();

        foreach(var serial in AndroidDebugBridge.Devices()) {
            if (!serial.StartsWith("emulator-"))
                continue;
            runningAvds.Add(AndroidDebugBridge.EmuName(serial), serial);
        }

        if (Directory.Exists(avdHome)) {
            foreach (var file in Directory.GetFiles(avdHome, "*.ini")) {
                var ini = new IniFile(file);
                var name = Path.GetFileNameWithoutExtension(file);
                avds.Add(new DeviceData {
                    Name = name,
                    Serial = runningAvds.ContainsKey(name) ? runningAvds[name] : string.Empty,
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
                OSVersion = $"android-{AndroidDebugBridge.Shell(avd.Value, "getprop", "ro.build.version.sdk")}",
                IsRunning = true,
                IsEmulator = true,
                IsMobile = true
            });
        }

        return avds;
    }
    public static List<DeviceData> PhysicalDevices() {
        var runningDevices = AndroidDebugBridge.Devices();
        var devices = new List<DeviceData>();

        foreach(var serial in runningDevices) {
            if (serial.StartsWith("emulator-"))
                continue;

            devices.Add(new DeviceData {
                Name = AndroidDebugBridge.Shell(serial, "getprop", "ro.product.model"),
                OSVersion = $"android-{AndroidDebugBridge.Shell(serial, "getprop", "ro.build.version.sdk")}",
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
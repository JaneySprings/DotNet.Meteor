using DotNet.Debugging.Common.Android;
using DotNet.Meteor.Workspace.Extensions;

namespace DotNet.Meteor.Workspace.Devices;

public static class AndroidDeviceTool {
    public static List<DeviceData> VirtualDevices() {
        var runningAvds = new Dictionary<string, string>();
        var avds = new List<DeviceData>();
        var avdHome = GetEmulatorsDirectory();

        foreach (var serial in AndroidDebugBridge.GetDevices()) {
            if (!serial.StartsWith("emulator-"))
                continue;
            runningAvds.Add(AndroidEmulator.GetEmulatorName(serial), serial);
        }

        if (Directory.Exists(avdHome)) {
            foreach (var file in Directory.GetFiles(avdHome, "*.ini")) {
                var ini = new IniFile(file);
                var name = Path.GetFileNameWithoutExtension(file);
                avds.Add(new DeviceData(name, Platforms.Android) {
                    Serial = runningAvds.TryGetValue(name, out string? value) ? value : null,
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
            avds.Add(new DeviceData(avd.Key, Platforms.Android) {
                Serial = avd.Value,
                OSVersion = $"android-{AndroidDebugBridge.Shell(avd.Value, "getprop", "ro.build.version.sdk")}",
                IsRunning = true,
                IsEmulator = true,
                IsMobile = true
            });
        }

        return avds;
    }
    public static HashSet<DeviceData> PhysicalDevices() {
        var runningDevices = AndroidDebugBridge.GetDevices();
        var devices = new HashSet<DeviceData>(DeviceDataEqualityComparer.Instance);

        foreach (var serial in runningDevices) {
            if (serial.StartsWith("emulator-"))
                continue;

            var roSerial = AndroidDebugBridge.Shell(serial, "getprop", "ro.serialno");
            if (string.IsNullOrEmpty(roSerial))
                roSerial = serial;

            devices.Add(new DeviceData(AndroidDebugBridge.Shell(serial, "getprop", "ro.product.model"), Platforms.Android) {
                OSVersion = $"android-{AndroidDebugBridge.Shell(serial, "getprop", "ro.build.version.sdk")}",
                IsEmulator = false,
                IsRunning = true,
                IsMobile = true,
                Serial = roSerial
            });
        }

        return devices;
    }

    private static string GetEmulatorsDirectory() {
        var path = Environment.GetEnvironmentVariable("ANDROID_AVD_HOME");
        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            return path;

        return Path.Combine(RuntimeInfo.HomeDirectory, ".android", "avd");
    }
}
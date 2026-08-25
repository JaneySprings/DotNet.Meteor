using DotNet.Meteor.Workspace.Extensions;

namespace DotNet.Meteor.Workspace.Devices;

public static class AppleDeviceTool {
    public static List<DeviceData> VirtualDevices() {
        var devices = new List<DeviceData>();
        var runtimeId = RuntimeInfo.IsAarch64
            ? Runtimes.iOSSimulatorArm64
            : Runtimes.iOSSimulatorX64;

        foreach (string directory in Directory.EnumerateDirectories(GetSimulatorsDirectory())) {
            var plist = Path.Combine(directory, "device.plist");

            if (!File.Exists(plist))
                continue;

            var extractor = new PropertyExtractor(plist);
            if (extractor.ExtractBoolean("isDeleted"))
                continue;

            var runtime = extractor.Extract("runtime");
            var osVersion = "Unknown";
            if (runtime?.Contains("SimRuntime.iOS") != true)
                continue;

            var tokens = runtime.Split('.').Last().Split('-');
            if (tokens.Length > 1)
                osVersion = $"{tokens[0]} {string.Join('.', tokens.Skip(1))}";

            devices.Add(new DeviceData(extractor.Extract("name") ?? "Unknown", Categories.iOSSimulator, Platforms.iOS) {
                IsEmulator = true,
                IsMobile = true,
                IsRunning = extractor.Extract("state", "integer")?.Equals("3") == true,
                RuntimeId = runtimeId,
                OSVersion = osVersion,
                Serial = extractor.Extract("UDID") ?? string.Empty
            });
            extractor.Free();
        }

        return devices;
    }
    public static List<DeviceData> PhysicalDevices() {
        return SystemProfiler.PhysicalDevices();
    }
    public static List<DeviceData> MacintoshDevices() {
        var devices = new List<DeviceData>();
        var tokens = Environment.OSVersion.VersionString.Split(' ');
        devices.Add(new DeviceData(Environment.MachineName, Categories.MacCatalyst, Platforms.MacCatalyst) {
            IsEmulator = false,
            IsRunning = true,
            IsMobile = false,
            // Xamarin shows 'missing rid' error for maccatalyst-arm64
            // RuntimeId = RuntimeInfo.IsAarch64 ? Runtimes.MacArm64 : Runtimes.MacX64,
            OSVersion = $"MacOS {tokens.Last()}",
        });

        return devices;
    }

    private static string GetSimulatorsDirectory() {
        var home = Environment.GetEnvironmentVariable("HOME")!;
        var path = Path.Combine(home, "Library", "Developer", "CoreSimulator", "Devices");

        if (string.IsNullOrEmpty(path))
            throw new InvalidOperationException("Could not find simulator path");

        return path;
    }
}
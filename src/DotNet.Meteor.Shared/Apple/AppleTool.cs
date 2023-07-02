using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Apple {
    public static class AppleTool {
        public static List<DeviceData> VirtualDevices() {
            var devices = new List<DeviceData>();
            var path = PathUtils.SimulatorsLocation();
            var runtimeId = SystemProfiler.IsArch64()
                ? Runtimes.iOSSimulatorArm64
                : Runtimes.iOSSimulatorX64;

            foreach (string directory in Directory.GetDirectories(path)) {
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

                devices.Add(new DeviceData {
                    IsEmulator = true,
                    IsMobile = true,
                    IsRunning = extractor.Extract("state", "integer")?.Equals("3") == true,
                    Name = extractor.Extract("name") ?? "Unknown",
                    RuntimeId = runtimeId,
                    Detail = Details.iOSSimulator,
                    Platform = Platforms.iOS,
                    OSVersion = osVersion,
                    Serial = extractor.Extract("UDID")
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
            var osVersion = $"MacOS {tokens.Last()}";

            if (SystemProfiler.IsArch64()) {
                devices.Add(new DeviceData {
                    IsEmulator = false,
                    IsRunning = true,
                    IsMobile = false,
                    RuntimeId = Runtimes.MacArm64,
                    OSVersion = osVersion,
                    Detail = Details.MacCatalyst,
                    Platform = Platforms.MacCatalyst,
                    Name = $"{Environment.MachineName} ({Details.MacArm})"
                });
            }

            devices.Add(new DeviceData {
                IsEmulator = false,
                IsRunning = true,
                IsMobile = false,
                RuntimeId = Runtimes.MacX64,
                OSVersion = osVersion,
                Detail = Details.MacCatalyst,
                Platform = Platforms.MacCatalyst,
                Name = $"{Environment.MachineName} ({Details.MacX64})"
            });

            return devices;
        }
    }
}
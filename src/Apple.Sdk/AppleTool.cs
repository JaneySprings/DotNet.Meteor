using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DotNet.Meteor.Shared;

namespace Apple.Sdk {
    public static class AppleTool {
        public static List<DeviceData> SimulatorsFast() {
            var devices = new List<DeviceData>();
            var path = PathUtils.SimulatorsLocation();

            foreach (string directory in Directory.GetDirectories(path)) {
                var plist = Path.Combine(directory, "device.plist");

                if (!File.Exists(plist))
                    continue;

                var extractor = PropertyExtractor.FromFile(plist);

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
                    RuntimeId = Runtimes.iOSimulatorX64,
                    Details = Details.iOSSimulator,
                    Platform = Platforms.iOS,
                    OSVersion = osVersion,
                    Serial = extractor.Extract("UDID")
                });
                extractor.Free();
            }

            return devices;
        }

        public static DeviceData MacDevice() {
            var runtime = SystemProfiler.IsArch64() ? Runtimes.MacArm64 : Runtimes.MacX64;
            var tokens = Environment.OSVersion.VersionString.Split(' ');
            var osVersion = $"MacOS {tokens.Last()}";

            return new DeviceData {
                IsEmulator = false,
                IsRunning = true,
                IsMobile = false,
                RuntimeId = runtime,
                OSVersion = osVersion,
                Details = Details.MacCatalyst,
                Name = Environment.MachineName,
                Platform = Platforms.MacCatalyst
            };
        }

        public static List<DeviceData> AllDevices() {
            var devices = new List<DeviceData>();
            devices.AddRange(SystemProfiler.PhysicalDevices().OrderBy(x => x.Name));
            devices.AddRange(SimulatorsFast().OrderBy(x => x.Name));
            return devices;
        }

        public static bool TryGetDevices(List<DeviceData> devices) {
            try {
                devices.AddRange(AllDevices());
                return true;
            } catch {
                return false;
            }
        }
    }
}
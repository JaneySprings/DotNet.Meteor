using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotNet.Mobile.Shared;
using System;

namespace Apple.Sdk {
    public static class SystemProfiler {
        public static List<DeviceData> PhysicalDevices() {
            var profiler = PathUtils.SystemProfilerTool();
            var devices = new List<DeviceData>();
            var regex = new Regex(@"(iPhone:)[^,]*?Version:\s+(?<ver>\d+.\d+)[^,]*?Serial\sNumber:\s+(?<id>\S+)");

            ProcessResult result = new ProcessRunner(profiler, new ProcessArgumentBuilder()
                .Append("SPUSBDataType"))
                .WaitForExit();

            var output = string.Join(Environment.NewLine, result.StandardOutput);

            foreach (Match match in regex.Matches(output)) {
                var version = match.Groups["ver"].Value;
                var serial = match.Groups["id"].Value.Insert(8, "-");

                devices.Add(new DeviceData {
                    IsEmulator = false,
                    IsRunning = true,
                    IsMobile = true,
                    Name = $"iPhone {version}",
                    Details = Details.iOSDevice,
                    Platform = Platforms.iOS,
                    Serial = serial
                });
            }
            return devices;
        }

        public static DeviceData MacDevice() {
            var profiler = PathUtils.SystemProfilerTool();
            string version = null;
            string name = null;

            ProcessResult result = new ProcessRunner(profiler, new ProcessArgumentBuilder()
                .Append("SPSoftwareDataType"))
                .WaitForExit();

            var output = string.Join(Environment.NewLine, result.StandardOutput);
            var nameMatch = new Regex(@"Computer Name: *(?<name>.+)").Match(output);
            var versionMatch = new Regex(@"System Version: *(?<os>.+)").Match(output);

            if (nameMatch.Success)
                name = nameMatch.Groups["name"].Value;
            if (versionMatch.Success)
                version = versionMatch.Groups["os"].Value;

            return new DeviceData {
                IsEmulator = false,
                IsRunning = true,
                IsMobile = false,
                IsArm = IsArch64(),
                Name = name,
                OSVersion = version,
                Details = Details.MacCatalyst,
                Platform = Platforms.MacCatalyst
            };
        }

        private static bool IsArch64() {
            var profiler = PathUtils.SystemProfilerTool();
            ProcessResult result = new ProcessRunner(profiler, new ProcessArgumentBuilder()
                .Append("SPHardwareDataType"))
                .WaitForExit();

            var output = string.Join(Environment.NewLine, result.StandardOutput);
            var appleSilicon = new Regex(@"Chip: *(?<name>.+)").Match(output);

            return appleSilicon.Success;
        }
    }
}
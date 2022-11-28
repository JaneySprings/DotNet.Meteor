using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Text.RegularExpressions;
using DotNet.Mobile.Shared;

namespace XCode.Sdk {
    public static class XCodeTool {
        public static List<DeviceData> PhysicalDevicesFast() {
            var profiler = PathUtils.GetSystemProfiler();
            var devices = new List<DeviceData>();
            var regex = new Regex(@"(iPhone:)[^,]*?Version:\s+(?<ver>\d+)[^,]*?Serial\sNumber:\s+(?<id>\S+)");

            ProcessResult result = ProcessRunner.Run(profiler, new ProcessArgumentBuilder()
                .Append("SPUSBDataType")
            );
            var output = string.Join(Environment.NewLine, result.StandardOutput);

            foreach (Match match in regex.Matches(output)) {
                var version = match.Groups["ver"].Value;
                var serial = match.Groups["id"].Value.Insert(8, "-");

                devices.Add(new DeviceData {
                    IsEmulator = false,
                    IsRunning = true,
                    Name = $"iPhone {version}",
                    Details = "Device",
                    Platform = Platform.iOS,
                    OSVersion = "Unknown",
                    Serial = serial
                });
            }
            return devices;
        }

        public static List<DeviceData> AllDevices() {
            var devices = new List<DeviceData>();
            devices.AddRange(PhysicalDevicesFast());
            devices.AddRange(XCRun.Simulators());
            return devices;
        }
    }
}
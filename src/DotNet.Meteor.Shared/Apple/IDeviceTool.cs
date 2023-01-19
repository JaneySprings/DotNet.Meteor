using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Apple {
    public static class IDeviceTool {
        public static List<DeviceData> Info() {
            var devices = new List<DeviceData>();
            var tool = new FileInfo(Path.Combine(PathUtils.IDeviceLocation(), "ideviceinfo.exe"));
            var result = new ProcessRunner(tool).WaitForExit();

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            var regex = new Regex(@"^(\w+)\s*:\s*(.*)$");
            var output = string.Join(Environment.NewLine, result.StandardOutput);

            foreach (var match in regex.Matches(output)) {
                devices.Add(new DeviceData {
                    IsEmulator = false,
                    IsRunning = true,
                    IsMobile = true,
                    // Name = match.Groups[1].Value,
                    // Serial = match.Groups[1].Value,
                    // OSVersion = match.Groups[2].Value,
                    RuntimeId = Runtimes.iOSArm64,
                    Details = Details.iOSDevice,
                    Platform = Platforms.iOS
                });
            }

            return devices;
        }
    }
}
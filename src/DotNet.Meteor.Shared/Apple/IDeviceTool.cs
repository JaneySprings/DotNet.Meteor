using System;
using System.IO;
using System.Collections.Generic;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Apple {
    public static class IDeviceTool {
        public static DeviceData Info() {
            var tool = new FileInfo(Path.Combine(PathUtils.IDeviceLocation(), "ideviceinfo.exe"));
            var result = new ProcessRunner(tool).WaitForExit();

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            return new DeviceData {
                Name = FindValue(result.StandardOutput, "DeviceName"),
                Serial = FindValue(result.StandardOutput, "UniqueDeviceID"),
                OSVersion = "iOS " + FindValue(result.StandardOutput, "ProductVersion"),
                RuntimeId = Runtimes.iOSArm64,
                Details = Details.iOSDevice,
                Platform = Platforms.iOS,
                IsEmulator = false,
                IsRunning = true,
                IsMobile = true
            };
        }

        private static string FindValue(List<string> records, string key) {
            return records
                .Find(x =>x.StartsWith($"{key}:"))?
                .Replace($"{key}:", "")
                .Trim();
        }
    }
}
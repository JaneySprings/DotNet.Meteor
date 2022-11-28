using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using DotNet.Mobile.Shared;

namespace Android.Sdk {
    public static class DeviceBridge {
        public static FileInfo ToolLocation() {
            string sdk = PathUtils.GetSdkLocation();
            string path = Path.Combine(sdk, "platform-tools", "adb");

            if (!File.Exists(path))
                throw new Exception("Could not find adb tool");

            return new FileInfo(path);
        }

        public static string Shell(string serial, params string[] args) {
            var emulator = DeviceBridge.ToolLocation();
            var result = ProcessRunner.Run(emulator, new ProcessArgumentBuilder()
                .Append("-s", serial, "shell")
                .Append(args));

            return string.Join(Environment.NewLine, result.StandardOutput);
        }

        public static string Forward(string serial, int local, int target) {
            var emulator = DeviceBridge.ToolLocation();
            var result = ProcessRunner.Run(emulator, new ProcessArgumentBuilder()
                .Append("-s", serial, "forward")
                .Append($"tcp:{local}", $"tcp:{target}"));

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            return string.Join(Environment.NewLine, result.StandardOutput);
        }

        public static List<DeviceData> Devices() {
            var adb = DeviceBridge.ToolLocation();
            ProcessResult result = ProcessRunner.Run(adb, new ProcessArgumentBuilder()
                .Append("devices")
                .Append("-l"));

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            string regex = @"^(?<serial>\S+?)(\s+?)\s+(?<state>\S+)";
            var devices = new List<DeviceData>();

            foreach (string line in result.StandardOutput) {
                MatchCollection matches = Regex.Matches(line, regex, RegexOptions.Singleline);
                if (matches.Count == 0)
                    continue;

                string serial = matches.FirstOrDefault().Groups["serial"].Value;

                devices.Add(new DeviceData {
                    Name = serial,
                    Serial = serial,
                    Details = "Device",
                    OSVersion = "Unknown",
                    Platform = Platform.Android,
                    IsEmulator = serial.StartsWith("emulator-"),
                    IsRunning = true
                });
            }

            return devices;
        }

        public static string EmuName(string serial) {
            var adb = DeviceBridge.ToolLocation();
            ProcessResult result = ProcessRunner.Run(adb, new ProcessArgumentBuilder()
                .Append("-s", serial)
                .Append("emu", "avd", "name")
            );

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            return result.StandardOutput.FirstOrDefault();
        }
    }
}
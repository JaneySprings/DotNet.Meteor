using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using DotNet.Mobile.Shared;

namespace Android.Sdk {
    public static class DeviceBridge {
        public static string Shell(string serial, params string[] args) {
            var adb = PathUtils.AdbTool();
            var result = ProcessRunner.Execute(adb, new ProcessArgumentBuilder()
                .Append("-s", serial, "shell")
                .Append(args));

            return string.Join(Environment.NewLine, result.StandardOutput);
        }

        public static string Forward(string serial, int local, int target) {
            var adb = PathUtils.AdbTool();
            var result = ProcessRunner.Execute(adb, new ProcessArgumentBuilder()
                .Append("-s", serial, "forward")
                .Append($"tcp:{local}", $"tcp:{target}"));

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            return string.Join(Environment.NewLine, result.StandardOutput);
        }

        public static List<DeviceData> Devices() {
            var adb = PathUtils.AdbTool();
            ProcessResult result = ProcessRunner.Execute(adb, new ProcessArgumentBuilder()
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
            var adb = PathUtils.AdbTool();
            ProcessResult result = ProcessRunner.Execute(adb, new ProcessArgumentBuilder()
                .Append("-s", serial)
                .Append("emu", "avd", "name")
            );

            if (result.ExitCode != 0)
                return string.Empty;

            return result.StandardOutput.FirstOrDefault();
        }
    }
}
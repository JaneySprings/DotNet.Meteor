using System;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using DotNet.Mobile.Shared;

namespace Android.Sdk {
    public static class DeviceBridge {
        public static string Shell(string serial, params string[] args) {
            var adb = PathUtils.AdbTool();
            var result = new ProcessRunner(adb, new ProcessArgumentBuilder()
                .Append("-s", serial, "shell")
                .Append(args))
                .WaitForExit();

            return string.Join(Environment.NewLine, result.StandardOutput);
        }

        public static string Forward(string serial, int local, int target) {
            var adb = PathUtils.AdbTool();
            var result = new ProcessRunner(adb, new ProcessArgumentBuilder()
                .Append("-s", serial, "forward")
                .Append($"tcp:{local}", $"tcp:{target}"))
                .WaitForExit();

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            return string.Join(Environment.NewLine, result.StandardOutput);
        }

        public static List<DeviceData> Devices() {
            var adb = PathUtils.AdbTool();
            ProcessResult result = new ProcessRunner(adb, new ProcessArgumentBuilder()
                .Append("devices")
                .Append("-l"))
                .WaitForExit();

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
            ProcessResult result = new ProcessRunner(adb, new ProcessArgumentBuilder()
                .Append("-s", serial)
                .Append("emu", "avd", "name"))
                .WaitForExit();

            if (result.ExitCode != 0)
                return string.Empty;

            return result.StandardOutput.FirstOrDefault();
        }

        public static Process Logcat(string serial, IProcessLogger logger) {
            var adb = PathUtils.AdbTool();
            new ProcessRunner(adb, new ProcessArgumentBuilder()
                .Append("-s", serial)
                .Append("logcat")
                .Append("-c"))
                .WaitForExit();

            return new ProcessRunner(adb, new ProcessArgumentBuilder()
                .Append("-s", serial)
                .Append("logcat"),
                logger
            ).Start();
        }
    }
}
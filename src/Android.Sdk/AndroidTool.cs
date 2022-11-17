using DotNet.Mobile.Shared;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;

namespace Android.Sdk {
    public static class AndroidTool {
        public static List<VirtualDevice> GetVirtualDevices() {
            var avdManager = PathUtils.GetAVDManager();
            ProcessResult result = ProcessRunner.Run(avdManager, new ProcessArgumentBuilder()
                .Append("list")
                .Append("avds"));

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            string output = string.Join(Environment.NewLine, result.StandardOutput) + Environment.NewLine;
            string regex = @"(Name:\s+)(?<name>.*?)(\n).*?" +
                           @"(Path:\s+)(?<path>.*?)(\n).*?";
                        //    @"(Target:\s+)(?<target>.*?)(\n).*?" +
                        //    @"(Based on:\s+)(?<based>.*?)(\sTag/)" +
                        //    @"(ABI:\s+)(?<abi>.*?)(\n).*?";

            MatchCollection matches = Regex.Matches(output, regex, RegexOptions.Singleline);
            return matches.Select(m => new VirtualDevice {
                Name = m.Groups["name"].Value,
                Path = m.Groups["path"].Value
                // Target = m.Groups["target"].Value,
                // BasedOn = m.Groups["based"].Value,
                // ABI = m.Groups["abi"].Value
            }).ToList();
        }

        public static List<ActiveDevice> GetActiveDevices() {
            var adb = PathUtils.GetADBTool();
            ProcessResult result = ProcessRunner.Run(adb, new ProcessArgumentBuilder()
                .Append("devices")
                .Append("-l"));

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            string regex = @"^(?<serial>\S+?)(\s+?)\s+(?<state>\S+)";
            var devices = new List<ActiveDevice>();

            foreach (string line in result.StandardOutput) {
                MatchCollection matches = Regex.Matches(line, regex, RegexOptions.Singleline);

                if (matches.Count == 0)
                    continue;

                devices.Add(new ActiveDevice {
                    Serial = matches.FirstOrDefault().Groups["serial"].Value,
                    State = matches.FirstOrDefault().Groups["state"].Value,
                });
            }

            return devices;
        }

        public static List<DeviceData> GetAllDevices() {
            List<ActiveDevice> runningDevices = GetActiveDevices();
            List<DeviceData> devices = new List<DeviceData>();
            // Add running devices devices
            devices.AddRange(runningDevices.ConvertAll(d => d.ToDeviceData()));
            // Add virtual devices
            foreach(var avd in GetVirtualDevices()) {
                if (devices.Any(d => d.Name.Equals(avd.Name)))
                    continue;
                devices.Add(new DeviceData {
                    Name = avd.Name,
                    Details = "Emulator",
                    Serial = null,
                    Platform = Platform.Android,
                    IsEmulator = true,
                    IsRunning = false
                });
            }

            return devices;
        }

        public static string AdbShell(string serial, params string[] args) {
            var emulator = PathUtils.GetADBTool();
            var result = ProcessRunner.Run(emulator, new ProcessArgumentBuilder()
                .Append("-s", serial, "shell")
                .Append(args));

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            return string.Join(Environment.NewLine, result.StandardOutput);
        }

        public static string RunEmulator(string name) {
            var emulator = PathUtils.GetEmulatorTool();
            var process = new ProcessRunner(emulator, new ProcessArgumentBuilder()
                .Append("-avd")
                .Append(name));

            process.WaitForExitAsync();
            return Emulator.WaitForBoot();
        }
    }
}
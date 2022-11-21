using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using DotNet.Mobile.Shared;

namespace XCode.Sdk {
    public static class XCodeTool {
        public static List<DeviceData> GetAllDevices() {
            FileInfo tool = PathUtils.GetXCDeviceTool();
            ProcessResult result = ProcessRunner.Run(tool, new ProcessArgumentBuilder().Append("list"));

            string json = string.Join(Environment.NewLine, result.StandardOutput);
            List<Device> devices = JsonSerializer.Deserialize<List<Device>>(json);

            return devices
                .Where(d => d.Error == null)
                .Select(d => new DeviceData {
                    IsEmulator = d.Simulator,
                    IsRunning = false,
                    Name = d.Name,
                    Details = d.Simulator ? "Simulator" : "Device",
                    Platform = d.GetPlatformType(),
                    OSVersion = d.OSVersion,
                    Serial = d.Identifier
            }).ToList();
        }

        public static List<DeviceData> GetSimulators() {
            FileInfo tool = PathUtils.GetXCRunTool();
            ProcessResult result = ProcessRunner.Run(tool, new ProcessArgumentBuilder()
                .Append("simctl")
                .Append("list")
            );

            var output = string.Join(Environment.NewLine, result.StandardOutput);
            var contentRegex = new Regex(@"^--\s(?<os>.+)\s--\n(?<content>(\s{4}.+\n)*)", RegexOptions.Multiline);
            var deviceRegex = new Regex(@"^\s{4}(?<name>.+)\s\((?<udid>.+)\)\s\((?<state>.+)\)", RegexOptions.Multiline);
            var devices = new List<DeviceData>();

            foreach (Match match in contentRegex.Matches(output)) {
                var os = match.Groups["os"].Value;
                var content = match.Groups["content"].Value;

                foreach (Match deviceMatch in deviceRegex.Matches(content)) {
                    var state = deviceMatch.Groups["state"].Value;

                    devices.Add(new DeviceData {
                        IsEmulator = true,
                        IsRunning = state.Contains("Booted", StringComparison.OrdinalIgnoreCase),
                        Name = deviceMatch.Groups["name"].Value,
                        Details = "Simulator",
                        Platform = "ios",
                        OSVersion = os,
                        Serial = deviceMatch.Groups["udid"].Value
                    });
                }
            }

            return devices;
        }

        public static void StartDebugSession(string bundlePath, string deviceId, int port) {
            FileInfo tool = PathUtils.GetMLaunch();
            var process = new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append("--launchsim", $"\"{bundlePath}\"")
                .Append($"--argument=-monodevelop-port --argument={port} --setenv=__XAMARIN_DEBUG_PORT__={port}")
                .Append($"--device=:v2:udid={deviceId}")
            );
            process.WaitForExitAsync();
        }
    }
}
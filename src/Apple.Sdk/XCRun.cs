using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotNet.Mobile.Shared;

namespace Apple.Sdk {
    public static class XCRun {
        public static List<DeviceData> Simulators() {
            FileInfo tool = PathUtils.XCRunTool();
            ProcessResult result = new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append("simctl")
                .Append("list"))
                .WaitForExit();

            var output = string.Join(Environment.NewLine, result.StandardOutput);
            var contentRegex = new Regex(@"^--\s(?<os>iOS\s\d+(.\d+)+)\s--\n(?<content>(\s{4}.+\n)*)", RegexOptions.Multiline);
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
                        Details = "iPhoneSimulator",
                        Platform = "ios",
                        OSVersion = os,
                        Serial = deviceMatch.Groups["udid"].Value
                    });
                }
            }

            return devices;
        }

        public static List<DeviceData> PhysicalDevices() {
            FileInfo tool = PathUtils.XCRunTool();
            ProcessResult result = new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append("xctrace")
                .Append("list")
                .Append("devices"))
                .WaitForExit();

            var output = string.Join(Environment.NewLine, result.StandardOutput) + Environment.NewLine;
            var contentRegex = new Regex(@"^==\sDevices(\sOffline)*\s==\n(?<content>[^,]+?^\n)", RegexOptions.Multiline);
            var deviceRegex = new Regex(@"^(?<name>.+)\s\((?<os>.+)\)\s\((?<udid>.+)\)", RegexOptions.Multiline);
            var devices = new List<DeviceData>();

            foreach (Match match in contentRegex.Matches(output)) {
                var content = match.Groups["content"].Value;

                foreach (Match deviceMatch in deviceRegex.Matches(content)) {
                    devices.Add(new DeviceData {
                        IsEmulator = false,
                        IsRunning = true,
                        Name = deviceMatch.Groups["name"].Value,
                        Details = "Device",
                        Platform = "ios",
                        OSVersion = deviceMatch.Groups["os"].Value,
                        Serial = deviceMatch.Groups["udid"].Value
                    });
                }
            }

            return devices;
        }
    }
}
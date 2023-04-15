using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Apple {
    public static class XCRun {
        public static List<DeviceData> Simulators() {
            FileInfo tool = PathUtils.XCRunTool();
            ProcessResult result = new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append("simctl")
                .Append("list"))
                .WaitForExit();

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            var output = string.Join(Environment.NewLine, result.StandardOutput);
            var contentRegex = new Regex(@"^--\s(?<os>iOS\s\d+(.\d+)+)\s--\n(?<content>(\s{4}.+\n)*)", RegexOptions.Multiline);
            var deviceRegex = new Regex(@"^\s{4}(?<name>.+)\s\((?<udid>.+)\)\s\((?<state>.+)\)", RegexOptions.Multiline);
            var devices = new List<DeviceData>();
            var runtimeId = SystemProfiler.IsArch64() 
                ? Runtimes.iOSSimulatorArm64 
                : Runtimes.iOSSimulatorX64;

            foreach (Match match in contentRegex.Matches(output)) {
                var os = match.Groups["os"].Value;
                var content = match.Groups["content"].Value;

                foreach (Match deviceMatch in deviceRegex.Matches(content)) {
                    var state = deviceMatch.Groups["state"].Value;

                    devices.Add(new DeviceData {
                        IsEmulator = true,
                        IsMobile = true,
                        IsRunning = state.Contains("Booted", StringComparison.OrdinalIgnoreCase),
                        Name = deviceMatch.Groups["name"].Value,
                        Detail = Details.iOSSimulator,
                        Platform = Platforms.iOS,
                        RuntimeId = runtimeId,
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

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

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
                        IsMobile = true,
                        Name = deviceMatch.Groups["name"].Value,
                        Detail = Details.iOSDevice,
                        Platform = Platforms.iOS,
                        RuntimeId = Runtimes.iOSArm64,
                        OSVersion = deviceMatch.Groups["os"].Value,
                        Serial = deviceMatch.Groups["udid"].Value
                    });
                }
            }

            return devices;
        }

        public static void ShutdownAll(IProcessLogger logger = null) {
            FileInfo tool = PathUtils.XCRunTool();
            ProcessResult result = new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append("simctl")
                .Append("shutdown")
                .Append("all"), logger)
                .WaitForExit();

            var output = string.Join(Environment.NewLine, result.StandardOutput) + Environment.NewLine;

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));
        }

        public static void LaunchSimulator(string serial, IProcessLogger logger = null) {
            var tool = PathUtils.OpenTool();
            ProcessResult result = new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append("-a", "Simulator")
                .Append("--args", "-CurrentDeviceUDID", serial), logger)
                .WaitForExit();

            var output = string.Join(Environment.NewLine, result.StandardOutput) + Environment.NewLine;

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));
        }
    }
}
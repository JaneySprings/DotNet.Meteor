using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotNet.Mobile.Shared;

namespace Android.Sdk {
    public static class VirtualDeviceManager {
        public static List<DeviceData> VirtualDevices() {
            var avdManager = PathUtils.AvdTool();
            ProcessResult result = ProcessRunner.Execute(avdManager, new ProcessArgumentBuilder()
                .Append("list")
                .Append("avds"));

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            string output = string.Join(Environment.NewLine, result.StandardOutput) + Environment.NewLine;
            string regex = @"(Name:\s+)(?<name>.*?)(\n).*?" +
                           @"(Based on:\s+)(?<based>.*?)(\sTag/)";

            MatchCollection matches = Regex.Matches(output, regex, RegexOptions.Singleline);
            return matches.Select(m => new DeviceData {
                Name = m.Groups["name"].Value,
                Details = "Emulator",
                Platform = Platform.Android,
                OSVersion = m.Groups["based"].Value,
                IsEmulator = true,
                IsRunning = false
            }).ToList();
        }
    }
}
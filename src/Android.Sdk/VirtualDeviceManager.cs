using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotNet.Mobile.Shared;

namespace Android.Sdk {
    public static class VirtualDeviceManager {
         public static FileInfo ToolLocation() {
            string sdk = PathUtils.GetSdkLocation();
            string tools = Path.Combine(sdk, "cmdline-tools");
            FileInfo newestTool = null;

            foreach (string directory in Directory.GetDirectories(tools)) {
                string avdPath = Path.Combine(directory, "bin", "avdmanager");

                if (File.Exists(avdPath)) {
                    var tool = new FileInfo(avdPath);

                    if (newestTool == null || tool.CreationTime > newestTool.CreationTime)
                        newestTool = tool;
                }
            }

            if (newestTool == null || !newestTool.Exists)
                throw new Exception("Could not find avdmanager tool");

            return newestTool;
        }

        public static List<DeviceData> VirtualDevices() {
            var avdManager = VirtualDeviceManager.ToolLocation();
            ProcessResult result = ProcessRunner.Run(avdManager, new ProcessArgumentBuilder()
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
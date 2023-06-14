using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Apple {
    public static class IDeviceTool {
        public static DeviceData Info() {
            var tool = new FileInfo(Path.Combine(PathUtils.IDeviceLocation(), "ideviceinfo.exe"));
            var result = new ProcessRunner(tool).WaitForExit();

            if (!result.Success)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            return new DeviceData {
                Name = FindValue(result.StandardOutput, "DeviceName"),
                Serial = FindValue(result.StandardOutput, "UniqueDeviceID"),
                OSVersion = "iOS " + FindValue(result.StandardOutput, "ProductVersion"),
                RuntimeId = Runtimes.iOSArm64,
                Detail = Details.iOSDevice,
                Platform = Platforms.iOS,
                IsEmulator = false,
                IsRunning = true,
                IsMobile = true
            };
        }

        public static void Installer(string serial, string bundlePath, IProcessLogger logger = null) {
            var tool = new FileInfo(Path.Combine(PathUtils.IDeviceLocation(), "ideviceinstaller.exe"));
            var result = new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append("--udid").Append(serial)
                .Append("--install").AppendQuoted(bundlePath)
                .Append("--notify-wait"), logger)
                .WaitForExit();

            if (!result.Success)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));
        }

        public static Process Debug(string serial, string bundleId, int port, IProcessLogger logger = null) {
            var tool = new FileInfo(Path.Combine(PathUtils.IDeviceLocation(), "idevicedebug.exe"));
            return new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append("run").Append(bundleId)
                .Append("--udid").Append(serial)
                .Append("--env").Append($"__XAMARIN_DEBUG_PORT__={port}")
                .Append("--debug"), logger)
                .Start();
        }


        private static string FindValue(List<string> records, string key) {
            return records
                .Find(x =>x.StartsWith($"{key}:"))?
                .Replace($"{key}:", "")
                .Trim();
        }
    }
}
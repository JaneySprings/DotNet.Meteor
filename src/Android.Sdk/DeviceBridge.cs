using System;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using DotNet.Meteor.Shared;

namespace Android.Sdk {
    public static class DeviceBridge {
        public static string Shell(string serial, params string[] args) {
            var adb = PathUtils.AdbTool();
            var result = new ProcessRunner(adb, new ProcessArgumentBuilder()
                .Append("-s", serial)
                .Append("shell")
                .Append(args))
                .WaitForExit();

            if (result.ExitCode != 0)
                string.Join(Environment.NewLine, result.StandardError);

            return string.Join(Environment.NewLine, result.StandardOutput);
        }

        public static string Forward(string serial, int local, int target) {
            var adb = PathUtils.AdbTool();
            var result = new ProcessRunner(adb, new ProcessArgumentBuilder()
                .Append("-s", serial)
                .Append("forward")
                .Append($"tcp:{local}")
                .Append($"tcp:{target}"))
                .WaitForExit();

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            return string.Join(Environment.NewLine, result.StandardOutput);
        }

        public static void Install(string serial, string apk, IProcessLogger logger = null) {
            var adb = PathUtils.AdbTool();
            var arguments = new ProcessArgumentBuilder()
                .Append("-s", serial)
                .Append("install")
                .Append("-r").AppendQuoted(apk);

            var result = new ProcessRunner(adb, arguments, logger).WaitForExit();
            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));
        }

        public static void Uninstall(string serial, string pkg, IProcessLogger logger = null) {
            var adb = PathUtils.AdbTool();
            var argument = new ProcessArgumentBuilder()
                .Append("-s", serial)
                .Append("uninstall")
                .Append(pkg);
            new ProcessRunner(adb, argument, logger).WaitForExit();
        }

        public static void Launch(string serial, string pkg, IProcessLogger logger = null) {
            string result = Shell(serial, "monkey", "-p", pkg, "1");
            logger?.OnOutputDataReceived(result);
        }

        public static List<string> Devices() {
            var adb = PathUtils.AdbTool();
            ProcessResult result = new ProcessRunner(adb, new ProcessArgumentBuilder()
                .Append("devices")
                .Append("-l"))
                .WaitForExit();

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            string regex = @"^(?<serial>\S+?)(\s+?)\s+(?<state>\S+)";
            var devices = new List<string>();

            foreach (string line in result.StandardOutput) {
                MatchCollection matches = Regex.Matches(line, regex, RegexOptions.Singleline);
                if (matches.Count == 0)
                    continue;

                devices.Add(matches.FirstOrDefault().Groups["serial"].Value);
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

        public static string DevName(string serial) {
            return Shell(serial, "getprop", "ro.product.model");
        }

        public static Process Logcat(string serial, IProcessLogger logger) {
            var adb = PathUtils.AdbTool();
            new ProcessRunner(adb, new ProcessArgumentBuilder()
                .Append("-s", serial)
                .Append("logcat")
                .Append("-c"))
                .WaitForExit();

            var arguments = new ProcessArgumentBuilder()
                .Append("-s", serial)
                .Append("logcat")
                .Append("-v", "tag");
            return new ProcessRunner(adb, arguments, logger).Start();
        }
    }
}
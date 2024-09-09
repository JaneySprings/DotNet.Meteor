using System.Diagnostics;
using System.Text.RegularExpressions;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Common;

namespace DotNet.Meteor.Debug.Sdk;

public static class DeviceBridge {
    public static string Shell(string serial, params string[] args) {
        var adb = AndroidSdk.AdbTool();
        var result = new ProcessRunner(adb, new ProcessArgumentBuilder()
            .Append("-s", serial)
            .Append("shell")
            .Append(args))
            .WaitForExit();

        if (!result.Success)
            return string.Join(Environment.NewLine, result.StandardError);

        return string.Join(Environment.NewLine, result.StandardOutput);
    }

    public static string Forward(string serial, int port) {
        var adb = AndroidSdk.AdbTool();
        var result = new ProcessRunner(adb, new ProcessArgumentBuilder()
            .Append("-s", serial)
            .Append("forward")
            .Append($"tcp:{port}")
            .Append($"tcp:{port}"))
            .WaitForExit();

        return string.Join(Environment.NewLine, result.StandardOutput);
    }
    public static string Reverse(string serial, int target, int destination) {
        var adb = AndroidSdk.AdbTool();
        var result = new ProcessRunner(adb, new ProcessArgumentBuilder()
            .Append("-s", serial)
            .Append("reverse")
            .Append($"tcp:{target}")
            .Append($"tcp:{destination}"))
            .WaitForExit();

        return string.Join(Environment.NewLine, result.StandardOutput);
    }
    public static string RemoveForward(string serial) {
        var adb = AndroidSdk.AdbTool();
        var result = new ProcessRunner(adb, new ProcessArgumentBuilder()
            .Append("-s", serial)
            .Append("forward")
            .Append("--remove-all"))
            .WaitForExit();

        if (!result.Success)
            throw new Exception(string.Join(Environment.NewLine, result.StandardError));

        return string.Join(Environment.NewLine, result.StandardOutput);
    }
    public static string RemoveReverse(string serial) {
        var adb = AndroidSdk.AdbTool();
        var result = new ProcessRunner(adb, new ProcessArgumentBuilder()
            .Append("-s", serial)
            .Append("reverse")
            .Append("--remove-all"))
            .WaitForExit();

        if (!result.Success)
            throw new Exception(string.Join(Environment.NewLine, result.StandardError));

        return string.Join(Environment.NewLine, result.StandardOutput);
    }

    public static void Install(string serial, string apk, IProcessLogger? logger = null) {
        var adb = AndroidSdk.AdbTool();
        var arguments = new ProcessArgumentBuilder()
            .Append("-s", serial)
            .Append("install")
            .AppendQuoted(apk);

        var result = new ProcessRunner(adb, arguments, logger).WaitForExit();
        if (!result.Success)
            throw new Exception(string.Join(Environment.NewLine, result.StandardError));
    }
    public static void Uninstall(string serial, string pkg, IProcessLogger? logger = null) {
        var adb = AndroidSdk.AdbTool();
        var argument = new ProcessArgumentBuilder()
            .Append("-s", serial)
            .Append("uninstall")
            .Append(pkg);
        new ProcessRunner(adb, argument, logger).WaitForExit();
    }
    public static void Launch(string serial, string pkg, IProcessLogger? logger = null) {
        // This is a legacy method that is no longer used (device auto-rotation issue).
        // string result = Shell(serial, "monkey", "--pct-syskeys", "0", "-p", pkg, "1");
        string result = Shell(serial, "am", "start", $"{pkg}/$(cmd package resolve-activity -c android.intent.category.LAUNCHER {pkg} | sed -n '/name=/s/^.*name=//p')");
        logger?.OnOutputDataReceived(result);
    }

    public static void Flush(string serial) {
        var adb = AndroidSdk.AdbTool();
        new ProcessRunner(adb, new ProcessArgumentBuilder()
            .Append("-s", serial)
            .Append("logcat")
            .Append("-c"))
            .WaitForExit();
    }
    public static Process Logcat(string serial, string buffer, string filter, IProcessLogger logger) {
        var adb = AndroidSdk.AdbTool();
        var arguments = new ProcessArgumentBuilder()
            .Append("-s", serial)
            .Append("logcat")
            .Append("-s", filter)
            .Append("-b", buffer)
            .Append("-v", "tag");
        return new ProcessRunner(adb, arguments, logger).Start();
    }
    public static Process Logcat(string serial, IProcessLogger logger) {
        var adb = AndroidSdk.AdbTool();
        var arguments = new ProcessArgumentBuilder()
            .Append("-s", serial)
            .Append("logcat")
            .Append("-v", "tag");
        return new ProcessRunner(adb, arguments, logger).Start();
    }

    public static List<string> Devices() {
        var adb = AndroidSdk.AdbTool();
        ProcessResult result = new ProcessRunner(adb, new ProcessArgumentBuilder()
            .Append("devices")
            .Append("-l"))
            .WaitForExit();

        if (!result.Success)
            throw new Exception(string.Join(Environment.NewLine, result.StandardError));

        string regex = @"^(?<serial>\S+?)(\s+?)\s+(?<state>\S+)";
        var devices = new List<string>();

        foreach (string line in result.StandardOutput) {
            MatchCollection matches = Regex.Matches(line, regex, RegexOptions.Singleline);
            if (matches.Count == 0)
                continue;

            devices.Add(matches[0].Groups["serial"].Value);
        }

        return devices;
    }
    public static string? EmuName(string serial) {
        var adb = AndroidSdk.AdbTool();
        ProcessResult result = new ProcessRunner(adb, new ProcessArgumentBuilder()
            .Append("-s", serial)
            .Append("emu", "avd", "name"))
            .WaitForExit();

        if (!result.Success)
            return string.Empty;

        return result.StandardOutput.FirstOrDefault();
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Debug.Sdk;

public static class DeviceBridge {
    public static string Shell(string serial, params string[] args) {
        var adb = AndroidUtilities.AdbTool();
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
        var adb = AndroidUtilities.AdbTool();
        var result = new ProcessRunner(adb, new ProcessArgumentBuilder()
            .Append("-s", serial)
            .Append("forward")
            .Append($"tcp:{port}")
            .Append($"tcp:{port}"))
            .WaitForExit();

        if (!result.Success)
            throw new Exception(string.Join(Environment.NewLine, result.StandardError));

        return string.Join(Environment.NewLine, result.StandardOutput);
    }

    public static void Install(string serial, string apk, IProcessLogger logger = null) {
        var adb = AndroidUtilities.AdbTool();
        var arguments = new ProcessArgumentBuilder()
            .Append("-s", serial)
            .Append("install")
            .AppendQuoted(apk);

        var result = new ProcessRunner(adb, arguments, logger).WaitForExit();
        if (!result.Success)
            throw new Exception(string.Join(Environment.NewLine, result.StandardError));
    }

    public static void Uninstall(string serial, string pkg, IProcessLogger logger = null) {
        var adb = AndroidUtilities.AdbTool();
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

    public static void Flush(string serial) {
        var adb = AndroidUtilities.AdbTool();
        new ProcessRunner(adb, new ProcessArgumentBuilder()
            .Append("-s", serial)
            .Append("logcat")
            .Append("-c"))
            .WaitForExit();
    }

    public static Process Logcat(string serial, string buffer, string filter, IProcessLogger logger) {
        var adb = AndroidUtilities.AdbTool();
        var arguments = new ProcessArgumentBuilder()
            .Append("-s", serial)
            .Append("logcat")
            .Append("-s", filter)
            .Append("-b", buffer)
            .Append("-v", "tag");
        return new ProcessRunner(adb, arguments, logger).Start();
    }

    public static List<string> Devices() {
        var adb = AndroidUtilities.AdbTool();
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

    public static string EmuName(string serial) {
        var adb = AndroidUtilities.AdbTool();
        ProcessResult result = new ProcessRunner(adb, new ProcessArgumentBuilder()
            .Append("-s", serial)
            .Append("emu", "avd", "name"))
            .WaitForExit();

        if (!result.Success)
            return string.Empty;

        return result.StandardOutput.FirstOrDefault();
    }
}
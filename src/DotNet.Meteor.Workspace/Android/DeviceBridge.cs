using System.Text.RegularExpressions;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Workspace.Android;

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

            devices.Add(matches.First().Groups["serial"].Value);
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

        return result.StandardOutput.FirstOrDefault() ?? string.Empty;
    }
}
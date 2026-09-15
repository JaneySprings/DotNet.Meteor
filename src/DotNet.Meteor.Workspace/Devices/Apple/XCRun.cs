using System.Text.RegularExpressions;
using DotNet.Debugging.Common.Interop;

namespace DotNet.Meteor.Workspace.Devices;

public static class XCRun {
    public static List<DeviceData> Simulators() {
        ProcessResult result = new ProcessRunner(GetXCRunPath(), new ProcessArgumentBuilder()
            .Append("simctl")
            .Append("list"))
            .WaitForExit();

        if (!result.Success)
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.StandardError));

        var output = string.Join(Environment.NewLine, result.StandardOutput);
        var contentRegex = new Regex(@"^--\s(?<os>iOS\s\d+(.\d+)+)\s--\n(?<content>(\s{4}.+\n)*)", RegexOptions.Multiline);
        var deviceRegex = new Regex(@"^\s{4}(?<name>.+)\s\((?<udid>.+)\)\s\((?<state>.+)\)", RegexOptions.Multiline);
        var devices = new List<DeviceData>();
        var runtimeId = RuntimeInfo.IsAarch64
            ? Runtimes.iOSSimulatorArm64
            : Runtimes.iOSSimulatorX64;

        foreach (Match match in contentRegex.Matches(output)) {
            var os = match.Groups["os"].Value;
            var content = match.Groups["content"].Value;

            foreach (Match deviceMatch in deviceRegex.Matches(content)) {
                var state = deviceMatch.Groups["state"].Value;

                devices.Add(new DeviceData(deviceMatch.Groups["name"].Value, Platforms.iOS) {
                    IsEmulator = true,
                    IsMobile = true,
                    IsRunning = state.Contains("Booted", StringComparison.OrdinalIgnoreCase),
                    RuntimeId = runtimeId,
                    OSVersion = os,
                    Serial = deviceMatch.Groups["udid"].Value
                });
            }
        }

        return devices;
    }
    public static List<DeviceData> PhysicalDevices() {
        ProcessResult result = new ProcessRunner(GetXCRunPath(), new ProcessArgumentBuilder()
            .Append("xctrace")
            .Append("list")
            .Append("devices"))
            .WaitForExit();

        if (!result.Success)
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.StandardError));

        var output = string.Join(Environment.NewLine, result.StandardOutput) + Environment.NewLine;
        var contentRegex = new Regex(@"^==\sDevices(\sOffline)*\s==\n(?<content>[^,]+?^\n)", RegexOptions.Multiline);
        var deviceRegex = new Regex(@"^(?<name>.+)\s\((?<os>.+)\)\s\((?<udid>.+)\)", RegexOptions.Multiline);
        var devices = new List<DeviceData>();

        foreach (Match match in contentRegex.Matches(output)) {
            var content = match.Groups["content"].Value;

            foreach (Match deviceMatch in deviceRegex.Matches(content)) {
                devices.Add(new DeviceData(deviceMatch.Groups["name"].Value, Platforms.iOS) {
                    IsEmulator = false,
                    IsRunning = true,
                    IsMobile = true,
                    RuntimeId = Runtimes.iOSArm64,
                    OSVersion = $"iOS {deviceMatch.Groups["os"].Value}",
                    Serial = deviceMatch.Groups["udid"].Value
                });
            }
        }

        return devices;
    }

    private static FileInfo GetXCRunPath() {
        string path = Path.Combine("/usr", "bin", "xcrun");
        FileInfo tool = new FileInfo(path);

        if (!tool.Exists)
            throw new InvalidOperationException("Could not find xcrun tool");

        return tool;
    }
}
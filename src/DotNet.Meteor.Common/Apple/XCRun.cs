using System.Text.RegularExpressions;
using DotNet.Meteor.Common.Processes;

namespace DotNet.Meteor.Common.Apple;

public static class XCRun {
    public static List<DeviceData> Simulators() {
        FileInfo tool = AppleSdkLocator.XCRunTool();
        ProcessResult result = new ProcessRunner(tool, new ProcessArgumentBuilder()
            .Append("simctl")
            .Append("list"))
            .WaitForExit();

        if (!result.Success)
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.StandardError));

        var output = string.Join(Environment.NewLine, result.StandardOutput);
        var contentRegex = new Regex(@"^--\s(?<os>iOS\s\d+(.\d+)+)\s--\n(?<content>(\s{4}.+\n)*)", RegexOptions.Multiline);
        var deviceRegex = new Regex(@"^\s{4}(?<name>.+)\s\((?<udid>.+)\)\s\((?<state>.+)\)", RegexOptions.Multiline);
        var devices = new List<DeviceData>();
        var runtimeId = RuntimeSystem.IsAarch64
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
        FileInfo tool = AppleSdkLocator.XCRunTool();
        ProcessResult result = new ProcessRunner(tool, new ProcessArgumentBuilder()
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
                devices.Add(new DeviceData {
                    IsEmulator = false,
                    IsRunning = true,
                    IsMobile = true,
                    Name = deviceMatch.Groups["name"].Value,
                    Detail = Details.iOSDevice,
                    Platform = Platforms.iOS,
                    RuntimeId = Runtimes.iOSArm64,
                    OSVersion = $"iOS {deviceMatch.Groups["os"].Value}",
                    Serial = deviceMatch.Groups["udid"].Value
                });
            }
        }

        return devices;
    }
    public static void ShutdownAll(IProcessLogger? logger = null) {
        FileInfo tool = AppleSdkLocator.XCRunTool();
        ProcessResult result = new ProcessRunner(tool, new ProcessArgumentBuilder()
            .Append("simctl")
            .Append("shutdown")
            .Append("all"), logger)
            .WaitForExit();

        var output = string.Join(Environment.NewLine, result.StandardOutput) + Environment.NewLine;

        if (!result.Success)
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.StandardError));
    }
    public static void LaunchSimulator(string serial, IProcessLogger? logger = null) {
        var tool = AppleSdkLocator.OpenTool();
        ProcessResult result = new ProcessRunner(tool, new ProcessArgumentBuilder()
            .Append("-a", "Simulator")
            .Append("--args", "-CurrentDeviceUDID", serial), logger)
            .WaitForExit();

        var output = string.Join(Environment.NewLine, result.StandardOutput) + Environment.NewLine;

        if (!result.Success)
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.StandardError));
    }
}
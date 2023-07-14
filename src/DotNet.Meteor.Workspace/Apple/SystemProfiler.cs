using System.Text.RegularExpressions;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Workspace.Apple;

public static class SystemProfiler {
    public static List<DeviceData> PhysicalDevices() {
        var profiler = AppleUtilities.SystemProfilerTool();
        var devices = new List<DeviceData>();
        var regex = new Regex(@"(?<dev>iPhone|iPad):[^,]*?Version:\s+(?<ver>\d+.\d+)[^,]*?Serial\sNumber:\s+(?<id>\S+)");

        ProcessResult result = new ProcessRunner(profiler, new ProcessArgumentBuilder()
            .Append("SPUSBDataType"))
            .WaitForExit();

        if (!result.Success)
            throw new Exception(string.Join(Environment.NewLine, result.StandardError));

        var output = string.Join(Environment.NewLine, result.StandardOutput);

        foreach (Match match in regex.Matches(output)) {
            var version = match.Groups["ver"].Value;
            var device = match.Groups["dev"].Value;
            var serial = match.Groups["id"].Value;
            //For modern iOS devices, the serial number is 24 characters long
            if (serial.Length == 24)
                serial = serial.Insert(8, "-");

            devices.Add(new DeviceData {
                IsEmulator = false,
                IsRunning = true,
                IsMobile = true,
                RuntimeId = Runtimes.iOSArm64,
                Name = $"{device} {version}",
                Detail = Details.iOSDevice,
                Platform = Platforms.iOS,
                Serial = serial
            });
        }
        return devices;
    }

    public static bool IsArch64() {
        var profiler = AppleUtilities.SystemProfilerTool();
        ProcessResult result = new ProcessRunner(profiler, new ProcessArgumentBuilder()
            .Append("SPHardwareDataType"))
            .WaitForExit();

        if (!result.Success)
            throw new Exception(string.Join(Environment.NewLine, result.StandardError));

        var output = string.Join(Environment.NewLine, result.StandardOutput);
        var appleSilicon = new Regex(@"Chip: *(?<name>.+)").Match(output);

        return appleSilicon.Success;
    }
}
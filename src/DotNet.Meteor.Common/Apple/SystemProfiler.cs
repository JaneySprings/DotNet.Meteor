using System.Text.RegularExpressions;
using DotNet.Meteor.Common.Processes;

namespace DotNet.Meteor.Common.Apple;

public static class SystemProfiler {
    public static List<DeviceData> PhysicalDevices() {
        var profiler = AppleSdkLocator.SystemProfilerTool();
        var devices = new List<DeviceData>();

        ProcessResult result = new ProcessRunner(profiler, new ProcessArgumentBuilder()
            .Append("SPUSBDataType"))
            .WaitForExit();

        var output = string.Join(Environment.NewLine, result.StandardOutput);
        if (string.IsNullOrWhiteSpace(output)) {
            result = new ProcessRunner(profiler, new ProcessArgumentBuilder()
                .Append("SPUSBHostDataType"))
                .WaitForExit();
            output = string.Join(Environment.NewLine, result.StandardOutput);
        }

        var regex = new Regex(@"(?<dev>iPhone|iPad):(?s:(?!iPhone:|iPad:).)*?(?:(?:Version:\s+(?<ver>[\w\.]+)(?s:(?!iPhone:|iPad:).)*?Serial\sNumber:\s+(?<id>\S+))|(?:Serial\sNumber:\s+(?<id>\S+)(?:(?s:(?!iPhone:|iPad:).)*?Version:\s+(?<ver>[\w\.]+))?))");
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
}
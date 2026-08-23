using System.Text.RegularExpressions;
using DotNet.Debugging.Common.Interop;

namespace DotNet.Meteor.Workspace.Devices;

public static class SystemProfiler {
    public static List<DeviceData> PhysicalDevices() {
        var profilerPath = GetStstemProfilerPath();
        var devices = new List<DeviceData>();

        ProcessResult result = new ProcessRunner(profilerPath, new ProcessArgumentBuilder()
            .Append("SPUSBDataType"))
            .WaitForExit();

        var output = string.Join(Environment.NewLine, result.StandardOutput);
        if (string.IsNullOrWhiteSpace(output)) {
            result = new ProcessRunner(profilerPath, new ProcessArgumentBuilder()
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
                Category = Categories.iOSDevice,
                Platform = Platforms.iOS,
                Serial = serial
            });
        }
        return devices;
    }

    private static FileInfo GetStstemProfilerPath() {
        string path = Path.Combine("/usr", "sbin", "system_profiler");
        var tool = new FileInfo(path);

        if (!tool.Exists)
            throw new InvalidOperationException("Could not find system_profiler path");

        return tool;
    }
}
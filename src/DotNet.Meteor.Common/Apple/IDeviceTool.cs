using System.Diagnostics;
using DotNet.Meteor.Common.Processes;

namespace DotNet.Meteor.Common.Apple;

public static class IDeviceTool {
    // This tool hangs on Windows, so we need to return a process to kill it.
    public static Process Installer(string serial, string bundlePath, IProcessLogger? logger = null) {
        var tool = new FileInfo(Path.Combine(AppleSdkLocator.IDeviceLocation(), "ideviceinstaller.exe"));
        return new ProcessRunner(tool, new ProcessArgumentBuilder()
            .Append("--udid").Append(serial)
            .Append("--install").AppendQuoted(bundlePath), logger)
            .Start();
    }
    public static Process Debug(string serial, string bundleId, int port, IProcessLogger? logger = null) {
        var tool = new FileInfo(Path.Combine(AppleSdkLocator.IDeviceLocation(), "idevicedebug.exe"));
        return new ProcessRunner(tool, new ProcessArgumentBuilder()
            .Append("run").Append(bundleId)
            .Append("--udid").Append(serial)
            .Append("--env").Append($"__XAMARIN_DEBUG_PORT__={port}")
            .Append("--debug"), logger)
            .Start();
    }
    public static IEnumerable<DeviceData> Info() {
        var tool = new FileInfo(Path.Combine(AppleSdkLocator.IDeviceLocation(), "ideviceinfo.exe"));
        var result = new ProcessRunner(tool).WaitForExit();

        if (!result.Success)
            return Enumerable.Empty<DeviceData>();

        return new List<DeviceData> {
            new DeviceData {
                Name = FindValue(result.StandardOutput, "DeviceName"),
                Serial = FindValue(result.StandardOutput, "UniqueDeviceID"),
                OSVersion = "iOS " + FindValue(result.StandardOutput, "ProductVersion"),
                RuntimeId = Runtimes.iOSArm64,
                Detail = Details.iOSDevice,
                Platform = Platforms.iOS,
                IsEmulator = false,
                IsRunning = true,
                IsMobile = true
            }
        };
    }

    private static string FindValue(List<string> records, string key) {
        return records
            .Find(x => x.StartsWith($"{key}:"))?
            .Replace($"{key}:", "")
            .Trim() ?? string.Empty;
    }
}
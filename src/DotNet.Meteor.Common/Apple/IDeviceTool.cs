using System.Diagnostics;
using DotNet.Meteor.Common.Processes;

namespace DotNet.Meteor.Common.Apple;

// This tool requires the 'Apple Devices' app daemon (AppleMobileDevice) or (usbmuxd) to be running.
// https://www.microsoft.com/store/productId/9NP83LWLPZ9K?ocid=pdpshare
public static class IDeviceTool {
    public static void Installer(string serial, string bundlePath, IProcessLogger? logger = null) {
        var tool = new FileInfo(Path.Combine(AppleSdkLocator.IDeviceLocation(), "ideviceinstaller" + RuntimeSystem.ExecExtension));
        var result = new ProcessRunner(tool, new ProcessArgumentBuilder()
            .Append("--udid").Append(serial)
            .Append("--install").AppendQuoted(bundlePath), logger)
            .WaitForExit();

        if (!result.Success)
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.StandardError));
    }
    public static IEnumerable<DeviceData> Info() {
        var tool = new FileInfo(Path.Combine(AppleSdkLocator.IDeviceLocation(), "ideviceinfo" + RuntimeSystem.ExecExtension));
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
    public static Process Proxy(string serial, int port, IProcessLogger? logger = null) {
        var tool = new FileInfo(Path.Combine(AppleSdkLocator.IDeviceLocation(), "iproxy" + RuntimeSystem.ExecExtension));
        var separator = RuntimeSystem.IsWindows ? ' ' : ':';
        return new ProcessRunner(tool, new ProcessArgumentBuilder()
            .Append($"{port}{separator}{port}")
            .Append(serial), logger)
            .Start();
    }


    private static string FindValue(List<string> records, string key) {
        return records
            .Find(x => x.StartsWith($"{key}:"))?
            .Replace($"{key}:", "")
            .Trim() ?? string.Empty;
    }
}
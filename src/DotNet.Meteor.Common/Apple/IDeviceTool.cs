using System.Diagnostics;
using DotNet.Meteor.Common.Processes;

namespace DotNet.Meteor.Common.Apple;

// This tool requires the 'Apple Devices' app daemon (AppleMobileDevice)
// https://www.microsoft.com/store/productId/9NP83LWLPZ9K?ocid=pdpshare
public static class IDeviceTool {
    public static void Installer(string serial, string bundlePath, IProcessLogger? logger = null) {
        var tool = new FileInfo(Path.Combine(AppleSdkLocator.IDeviceLocation(), "ideviceinstaller.exe"));
        var result = new ProcessRunner(tool, new ProcessArgumentBuilder()
            .Append("--udid").Append(serial)
            .Append("--install").AppendQuoted(bundlePath), logger)
            .WaitForExit();

        if (!result.Success)
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.StandardError));
    }
    public static Process Debug(string serial, string bundleId, int port, IProcessLogger? logger = null) {
        var tool = new FileInfo(Path.Combine(AppleSdkLocator.IDeviceLocation(), "idevicedebug.exe"));
        return new ProcessRunner(tool, new ProcessArgumentBuilder()
            .Append("run").Append(bundleId)
            .Append("--udid").Append(serial)
            .Append("--env").Append($"__XAMARIN_DEBUG_PORT__={port}"), logger)
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
    public static Process Proxy(string serial, int port, IProcessLogger? logger = null) {
        var tool = new FileInfo(Path.Combine(AppleSdkLocator.IDeviceLocation(), "iproxy.exe"));
        return new ProcessRunner(tool, new ProcessArgumentBuilder()
            .Append($"{port}:{port}")
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
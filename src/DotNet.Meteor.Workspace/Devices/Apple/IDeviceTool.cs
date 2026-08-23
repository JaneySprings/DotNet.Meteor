using DotNet.Debugging.Common.Apple;
using DotNet.Debugging.Common.Interop;

namespace DotNet.Meteor.Workspace.Devices;

// This tool requires the 'Apple Devices' app daemon (AppleMobileDevice) or (usbmuxd) to be running.
// https://www.microsoft.com/store/productId/9NP83LWLPZ9K?ocid=pdpshare
public static class IDeviceTool {
    public static IEnumerable<DeviceData> Info() {
        var tool = new FileInfo(Path.Combine(AppleSdkLocator.GetIDeviceDirectory(), "ideviceinfo" + RuntimeInfo.ExecExtension));
        var result = new ProcessRunner(tool).WaitForExit();

        if (!result.Success)
            return Enumerable.Empty<DeviceData>();

        return new List<DeviceData> {
            new DeviceData {
                Name = FindValue(result.StandardOutput, "DeviceName"),
                Serial = FindValue(result.StandardOutput, "UniqueDeviceID"),
                OSVersion = "iOS " + FindValue(result.StandardOutput, "ProductVersion"),
                RuntimeId = Runtimes.iOSArm64,
                Category = Categories.iOSDevice,
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
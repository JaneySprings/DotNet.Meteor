using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Workspace.Apple;

public static class IDeviceTool {
    public static DeviceData Info() {
        var tool = new FileInfo(Path.Combine(AppleSdk.IDeviceLocation(), "ideviceinfo.exe"));
        var result = new ProcessRunner(tool).WaitForExit();

        if (!result.Success)
            throw new Exception(string.Join(Environment.NewLine, result.StandardError));

        return new DeviceData {
            Name = FindValue(result.StandardOutput, "DeviceName"),
            Serial = FindValue(result.StandardOutput, "UniqueDeviceID"),
            OSVersion = "iOS " + FindValue(result.StandardOutput, "ProductVersion"),
            RuntimeId = Runtimes.iOSArm64,
            Detail = Details.iOSDevice,
            Platform = Platforms.iOS,
            IsEmulator = false,
            IsRunning = true,
            IsMobile = true
        };
    }


    private static string? FindValue(List<string> records, string key) {
        return records
            .Find(x =>x.StartsWith($"{key}:"))?
            .Replace($"{key}:", "")
            .Trim();
    }
}
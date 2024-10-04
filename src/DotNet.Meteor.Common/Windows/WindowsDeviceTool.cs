namespace DotNet.Meteor.Common.Windows;

public static class WindowsDeviceTool {
    public static DeviceData WindowsDevice() {
        string version = Environment.OSVersion.VersionString;
        string osVersion = version.Split(' ').Last();
        return new DeviceData {
            IsEmulator = false,
            IsRunning = true,
            IsMobile = false,
            Name = Environment.MachineName,
            OSVersion = osVersion,
            Detail = Details.Windows,
            Platform = Platforms.Windows
        };
    }
}
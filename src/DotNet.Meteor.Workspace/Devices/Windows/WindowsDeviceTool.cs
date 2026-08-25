namespace DotNet.Meteor.Workspace.Devices;

public static class WindowsDeviceTool {
    public static DeviceData WindowsDevice() {
        return new DeviceData(Environment.MachineName, Categories.Windows, Platforms.Windows) {
            IsEmulator = false,
            IsRunning = true,
            IsMobile = false,
            OSVersion = Environment.OSVersion.VersionString.Split(' ').Last(),
        };
    }
}
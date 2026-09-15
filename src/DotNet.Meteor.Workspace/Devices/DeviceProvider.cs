namespace DotNet.Meteor.Workspace.Devices;

public static class DeviceProvider {
    public static List<DeviceData> GetDevices(Action<Exception>? errorHandler = null, Action<string>? debugHandler = null) {
        var devices = new List<DeviceData>();
        debugHandler?.Invoke("Fetching devices...");

        try {
            if (RuntimeInfo.IsWindows) {
                devices.Add(WindowsDeviceTool.WindowsDevice());
                debugHandler?.Invoke("Windows device added.");
            }
        } catch (Exception e) { errorHandler?.Invoke(e); }

        try {
            devices.AddRange(AndroidDeviceTool.PhysicalDevices().OrderBy(x => x.Name));
            debugHandler?.Invoke("Android physical devices added.");

            devices.AddRange(AndroidDeviceTool.VirtualDevices().OrderBy(x => !x.IsRunning).ThenBy(x => x.Name));
            debugHandler?.Invoke("Android virtual devices added.");
        } catch (Exception e) { errorHandler?.Invoke(e); }

        try {
            if (RuntimeInfo.IsMacOS) {
                devices.AddRange(AppleDeviceTool.MacintoshDevices());
                debugHandler?.Invoke("MacOS devices added.");

                devices.AddRange(AppleDeviceTool.PhysicalDevices().OrderBy(x => x.Name));
                debugHandler?.Invoke("Apple physical devices added.");

                devices.AddRange(AppleDeviceTool.VirtualDevices().OrderBy(x => !x.IsRunning).ThenBy(x => x.Name));
                debugHandler?.Invoke("Apple virtual devices added.");
                // } else if (AppleSdkLocator.IsAppleDriverRunning()) {
                //     devices.AddRange(IDeviceTool.Info());
                //     debugHandler?.Invoke("iOS device added.");
            }
        } catch (Exception e) { errorHandler?.Invoke(e); }

        debugHandler?.Invoke($"Devices fetched. Total: {devices.Count}.");
        return devices;
    }
}
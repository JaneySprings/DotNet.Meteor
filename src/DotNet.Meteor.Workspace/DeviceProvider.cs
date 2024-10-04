using DotNet.Meteor.Common;
using DotNet.Meteor.Common.Android;
using DotNet.Meteor.Common.Apple;
using DotNet.Meteor.Common.Windows;

namespace DotNet.Meteor.Workspace;

public static class DeviceProvider {
    public static List<DeviceData> GetDevices(Action<Exception>? errorHandler = null, Action<string>? debugHandler = null) {
        var devices = new List<DeviceData>();
        debugHandler?.Invoke("Fetching devices...");

        try {
            if (RuntimeSystem.IsWindows) {
                devices.Add(WindowsDeviceTool.WindowsDevice());
                debugHandler?.Invoke("Windows device added.");
                //TODO: devices.Add(IDeviceTool.Info());
            }
        } catch (Exception e) { errorHandler?.Invoke(e); }

        try {
            devices.AddRange(AndroidDeviceTool.PhysicalDevices().OrderBy(x => x.Name));
            debugHandler?.Invoke("Android physical devices added.");

            devices.AddRange(AndroidDeviceTool.VirtualDevices().OrderBy(x => !x.IsRunning).ThenBy(x => x.Name));
            debugHandler?.Invoke("Android virtual devices added.");
        } catch (Exception e) { errorHandler?.Invoke(e); }

        try {
            if (RuntimeSystem.IsMacOS) {
                devices.AddRange(AppleDeviceTool.MacintoshDevices());
                debugHandler?.Invoke("MacOS devices added.");

                devices.AddRange(AppleDeviceTool.PhysicalDevices().OrderBy(x => x.Name));
                debugHandler?.Invoke("Apple physical devices added.");

                devices.AddRange(AppleDeviceTool.VirtualDevices().OrderBy(x => !x.IsRunning).ThenBy(x => x.Name));
                debugHandler?.Invoke("Apple virtual devices added.");
            }
        } catch (Exception e) { errorHandler?.Invoke(e); }

        debugHandler?.Invoke($"Devices fetched. Total: {devices.Count}.");
        return devices;
    }
}
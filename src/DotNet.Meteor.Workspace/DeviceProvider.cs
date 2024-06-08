using DotNet.Meteor.Common;
using DotNet.Meteor.Workspace.Apple;
using DotNet.Meteor.Workspace.Android;
using DotNet.Meteor.Workspace.Windows;

namespace DotNet.Meteor.Workspace;

public static class DeviceProvider {
    public static List<DeviceData> GetDevices(Action<Exception>? errorHandler = null, Action<string>? debugHandler = null) {
        var devices = new List<DeviceData>();
        debugHandler?.Invoke("Fetching devices...");

        try {
            if (RuntimeSystem.IsWindows) {
                devices.Add(WindowsTool.WindowsDevice());
                debugHandler?.Invoke("Windows device added.");
                //TODO: devices.Add(IDeviceTool.Info());
            }
        } catch (Exception e) { errorHandler?.Invoke(e); }

        try {
            devices.AddRange(AndroidTool.PhysicalDevices().OrderBy(x => x.Name));
            debugHandler?.Invoke("Android physical devices added.");

            devices.AddRange(AndroidTool.VirtualDevices().OrderBy(x => !x.IsRunning).ThenBy(x => x.Name));
            debugHandler?.Invoke("Android virtual devices added.");
        } catch (Exception e) { errorHandler?.Invoke(e); }

        try {
            if (RuntimeSystem.IsMacOS) {
                devices.AddRange(AppleTool.MacintoshDevices());
                debugHandler?.Invoke("MacOS devices added.");

                devices.AddRange(AppleTool.PhysicalDevices().OrderBy(x => x.Name));
                debugHandler?.Invoke("Apple physical devices added.");

                devices.AddRange(AppleTool.VirtualDevices().OrderBy(x => !x.IsRunning).ThenBy(x => x.Name));
                debugHandler?.Invoke("Apple virtual devices added.");
            }
        } catch (Exception e) { errorHandler?.Invoke(e); }

        debugHandler?.Invoke($"Devices fetched. Total: {devices.Count}.");
        return devices;
    }
}
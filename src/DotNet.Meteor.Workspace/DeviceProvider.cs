using DotNet.Meteor.Shared;
using DotNet.Meteor.Workspace.Apple;
using DotNet.Meteor.Workspace.Android;
using DotNet.Meteor.Workspace.Windows;

namespace DotNet.Meteor.Workspace;

public static class DeviceProvider {
    public static List<DeviceData> GetDevices(Action<Exception>? errorHandler = null) {
        var devices = new List<DeviceData>();

        try { /* Windows Devices */
            if (RuntimeSystem.IsWindows) {
                devices.Add(WindowsTool.WindowsDevice());
                devices.Add(IDeviceTool.Info());
            }
        } catch (Exception e) { errorHandler?.Invoke(e); }

        try { /* Android Devices */
            devices.AddRange(AndroidTool.PhysicalDevices().OrderBy(x => x.Name));
            devices.AddRange(AndroidTool.VirtualDevices().OrderBy(x => !x.IsRunning).ThenBy(x => x.Name));
        } catch (Exception e) { errorHandler?.Invoke(e); }

        try { /* Apple Devices */
            if (RuntimeSystem.IsMacOS) {
                devices.AddRange(AppleTool.MacintoshDevices());
                devices.AddRange(AppleTool.PhysicalDevices().OrderBy(x => x.Name));
                devices.AddRange(AppleTool.VirtualDevices().OrderBy(x => !x.IsRunning).ThenBy(x => x.Name));
            }
        } catch (Exception e) { errorHandler?.Invoke(e); }

        return devices;
    }
    }
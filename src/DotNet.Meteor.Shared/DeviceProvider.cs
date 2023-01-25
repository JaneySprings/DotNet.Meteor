using System;
using System.Collections.Generic;
using System.Linq;
using DotNet.Meteor.Android;
using DotNet.Meteor.Apple;
using DotNet.Meteor.Windows;
using NLog;

namespace DotNet.Meteor.Shared {
    public static class DeviceProvider {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static List<DeviceData> GetDevices() {
            var devices = new List<DeviceData>();

            try { /* Windows Devices */
                if (RuntimeSystem.IsWindows) {
                    devices.Add(WindowsTool.WindowsDevice());
                    devices.Add(IDeviceTool.Info());
                }
            } catch (Exception e) { Logger.Error(e); }

            try { /* Android Devices */
                devices.AddRange(AndroidTool.PhysicalDevices().OrderBy(x => x.Name));
                devices.AddRange(AndroidTool.VirtualDevices().OrderBy(x => x.Name));
            } catch (Exception e) { Logger.Error(e); }

            try { /* Apple Devices */
                if (RuntimeSystem.IsMacOS) {
                    devices.Add(AppleTool.MacintoshDevice());
                    devices.AddRange(AppleTool.PhysicalDevices().OrderBy(x => x.Name));
                    devices.AddRange(AppleTool.VirtualDevices().OrderBy(x => x.Name));
                }
            } catch (Exception e) { Logger.Error(e); }

            return devices;
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using DotNet.Meteor.Android;
using DotNet.Meteor.Apple;
using DotNet.Meteor.Windows;

namespace DotNet.Meteor.Shared {
    public static class DeviceProvider {
        public static List<DeviceData> GetDevices() {
            var devices = new List<DeviceData>();
            var logger = new Logger("environment");

            try { /* Windows Devices */
                if (RuntimeSystem.IsWindows) {
                    devices.Add(WindowsTool.WindowsDevice());
                    //devices.AddRange(/*Load apple devices from idevicelist.exe*/);
                }
            } catch (Exception e) {
                logger.Log(e);
            }

            try { /* Android Devices */
                devices.AddRange(AndroidTool.PhysicalDevices().OrderBy(x => x.Name));
                devices.AddRange(AndroidTool.VirtualDevices().OrderBy(x => x.Name));
            } catch (Exception e) {
                logger.Log(e);
            }

            try { /* Apple Devices */
                if (RuntimeSystem.IsMacOS) {
                    devices.Add(AppleTool.MacintoshDevice());
                    devices.AddRange(AppleTool.PhysicalDevices().OrderBy(x => x.Name));
                    devices.AddRange(AppleTool.VirtualDevices().OrderBy(x => x.Name));
                }
            } catch (Exception e) {
                logger.Log(e);
            }

            return devices;
        }

    }
}
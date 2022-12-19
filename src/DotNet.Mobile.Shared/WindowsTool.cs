using System;
using System.Linq;

namespace DotNet.Mobile.Shared {
    public static class WindowsTool {
        public static DeviceData WindowsDevice() {
            string version = Environment.OSVersion.VersionString;
            string osVersion = version.Split(' ').Last();
            return new DeviceData {
                IsEmulator = false,
                IsRunning = true,
                IsMobile = false,
                IsArm = false, //todo: check this
                Name = Environment.MachineName,
                OSVersion = osVersion,
                Details = Details.Windows,
                Platform = Platforms.Windows
            };
        }
    }
}
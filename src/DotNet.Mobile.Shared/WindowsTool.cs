using System;

namespace DotNet.Mobile.Shared {
    public static class WindowsTool {
        public static DeviceData WindowsDevice() {
            return new DeviceData {
                IsEmulator = false,
                IsRunning = true,
                IsMobile = false,
                IsArm = false, //todo: check this
                Name = Environment.MachineName,
                OSVersion = Environment.OSVersion.VersionString,
                Details = Details.Windows,
                Platform = Platforms.Windows
            };
        }
    }
}
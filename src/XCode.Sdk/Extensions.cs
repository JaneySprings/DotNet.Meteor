namespace XCode.Sdk {
    public static class DeviceExtensions {
        public static bool IsiOS(this Device device) {
            return !string.IsNullOrEmpty(device.Platform) && (device.Platform.Equals(Platforms.iPhoneDomain) || device.Platform.Equals(Platforms.iPhoneSimulatorDomain));
        }
        public static bool IsTVOS(this Device device) {
            return !string.IsNullOrEmpty(device.Platform) && (device.Platform.Equals(Platforms.AppleTVDomain) || device.Platform.Equals(Platforms.AppleTVSimulatorDomain));
        }
        public static bool IsWatchOS(this Device device) {
            return !string.IsNullOrEmpty(device.Platform) && (device.Platform.Equals(Platforms.AppleWatchDomain) || device.Platform.Equals(Platforms.AppleWatchSimulatorDomain));
        }
        public static bool IsOSX(this Device device) {
            return !string.IsNullOrEmpty(device.Platform) && device.Platform.Equals(Platforms.MacOSXDomain);
        }

        public static string GetPlatformType(this Device device) {
            if (device.IsiOS())
                return Platforms.iOSCode;

            if (device.IsTVOS())
                return Platforms.TVOSCode;

            if (device.IsWatchOS())
                return Platforms.WatchOSCode;

            if (device.IsOSX())
                return Platforms.MacOSXCode;

            return null;
        }
    }
}
﻿using System.Text.Json.Serialization;

namespace DotNet.Mobile.Shared {
    public class DeviceData {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("details")] public string Details { get; set; }
        [JsonPropertyName("serial")] public string Serial { get; set; }
        [JsonPropertyName("platform")] public string Platform { get; set; }
        [JsonPropertyName("os_version")] public string OSVersion { get; set; }
        [JsonPropertyName("is_emulator")] public bool IsEmulator { get; set; }
        [JsonPropertyName("is_running")] public bool IsRunning { get; set; }
        [JsonPropertyName("is_mobile")] public bool IsMobile { get; set; }
        [JsonPropertyName("is_arm")] public bool IsArm { get; set; }


        [JsonIgnore] public bool IsAndroid => Platform.Equals(Platforms.Android);
        [JsonIgnore] public bool IsIPhone => Platform.Contains(Platforms.iOS);
        [JsonIgnore] public bool IsMacCatalyst => Platform.Contains(Platforms.MacCatalyst);
        [JsonIgnore] public bool IsWindows => Platform.Contains(Platforms.Windows);
    }

    public static class Platforms {
        public const string Android = "android";
        public const string iOS = "ios";
        public const string MacCatalyst = "maccatalyst";
        public const string Windows = "windows";
    }

    public static class Details {
        public const string AndroidEmulator = "Emulator";
        public const string AndroidDevice = "Device";

        public const string iOSSimulator = "iPhoneSimulator";
        public const string iOSDevice = "iPhone";

        public const string MacCatalyst = "Mac";
        public const string Windows = "Windows";
    }
}
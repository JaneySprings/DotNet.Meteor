using System.Text.Json.Serialization;

namespace DotNet.Mobile.Shared {
    public class DeviceData {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("details")] public string Details { get; set; }
        [JsonPropertyName("serial")] public string Serial { get; set; }
        [JsonPropertyName("platform")] public string Platform { get; set; }
        [JsonPropertyName("runtime_id")] public string RuntimeId { get; set; }
        [JsonPropertyName("os_version")] public string OSVersion { get; set; }
        [JsonPropertyName("is_emulator")] public bool IsEmulator { get; set; }
        [JsonPropertyName("is_running")] public bool IsRunning { get; set; }
        [JsonPropertyName("is_mobile")] public bool IsMobile { get; set; }


        [JsonIgnore] public bool IsAndroid => Platform.Equals(Platforms.Android);
        [JsonIgnore] public bool IsIPhone => Platform.Equals(Platforms.iOS);
        [JsonIgnore] public bool IsMacCatalyst => Platform.Equals(Platforms.MacCatalyst);
        [JsonIgnore] public bool IsWindows => Platform.Equals(Platforms.Windows);
    }

    public static class Platforms {
        public const string Android = "android";
        public const string iOS = "ios";
        public const string MacCatalyst = "maccatalyst";
        public const string Windows = "windows";
    }

    public static class Runtimes {
        public const string MacArm64 = "maccatalyst-arm64";
        public const string MacX64 = "maccatalyst-x64";
        public const string iOSArm64 = "ios-arm64";
        public const string iOSimulatorX64 = "iossimulator-x64";
        public const string Unspecified = null;
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
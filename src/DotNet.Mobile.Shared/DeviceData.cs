using System.Text.Json.Serialization;

namespace DotNet.Mobile.Shared {
    public class DeviceData {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("details")] public string Details { get; set; }
        [JsonPropertyName("serial")] public string Serial { get; set; }
        [JsonPropertyName("platform")] public string Platform { get; set; }
        [JsonPropertyName("os_version")] public string OSVersion { get; set; }
        [JsonPropertyName("is_emulator")] public bool IsEmulator { get; set; }
        [JsonPropertyName("is_running")] public bool IsRunning { get; set; }

        public bool IsAndroid => Platform.Contains("android", System.StringComparison.OrdinalIgnoreCase);
        public bool IsIPhone => Platform.Contains("ios", System.StringComparison.OrdinalIgnoreCase);
    }

    public static class Platform {
        public const string Android = "android";
        public const string iOS = "ios";
    }
}
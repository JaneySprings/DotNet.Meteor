using System.Text.Json.Serialization;

namespace DotNet.Mobile.Shared {
    public class DeviceData {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("details")] public string Details { get; set; }
        [JsonPropertyName("serial")] public string Serial { get; set; }
        [JsonPropertyName ("platform")] public string Platform { get; set; }
        [JsonPropertyName("version")] public string Version { get; set; }
        [JsonPropertyName("is_emulator")] public bool IsEmulator { get; set; }
        [JsonPropertyName("is_running")] public bool IsRunning { get; set; }
        [JsonPropertyName("rid")] public string RuntimeIdentifier { get; set; }
    }

    public static class Platform {
        public const string Android = "android";
        public const string iOS = "ios";
    }
}
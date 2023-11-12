using System.Text.Json.Serialization;

namespace DotNet.Meteor.Shared {
    public class DeviceData {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("detail")] public string Detail { get; set; }
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
}
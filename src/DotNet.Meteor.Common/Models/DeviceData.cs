using System.Text.Json.Serialization;

namespace DotNet.Meteor.Common;

public class DeviceData {
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("detail")] public string Detail { get; set; } = string.Empty;
    [JsonPropertyName("serial")] public string Serial { get; set; } = string.Empty;
    [JsonPropertyName("platform")] public string Platform { get; set; } = string.Empty;
    [JsonPropertyName("runtime_id")] public string RuntimeId { get; set; } = string.Empty;
    [JsonPropertyName("os_version")] public string OSVersion { get; set; } = string.Empty;
    [JsonPropertyName("is_emulator")] public bool IsEmulator { get; set; }
    [JsonPropertyName("is_running")] public bool IsRunning { get; set; }
    [JsonPropertyName("is_mobile")] public bool IsMobile { get; set; }

    [JsonIgnore] public bool IsAndroid => Platform.Equals(Platforms.Android);
    [JsonIgnore] public bool IsIPhone => Platform.Equals(Platforms.iOS);
    [JsonIgnore] public bool IsMacCatalyst => Platform.Equals(Platforms.MacCatalyst);
    [JsonIgnore] public bool IsWindows => Platform.Equals(Platforms.Windows);
}
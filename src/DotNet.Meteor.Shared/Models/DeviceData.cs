using Newtonsoft.Json;

namespace DotNet.Meteor.Shared {
    public class DeviceData {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("details")] public string Details { get; set; }
        [JsonProperty("serial")] public string Serial { get; set; }
        [JsonProperty("platform")] public string Platform { get; set; }
        [JsonProperty("runtime_id")] public string RuntimeId { get; set; }
        [JsonProperty("os_version")] public string OSVersion { get; set; }
        [JsonProperty("is_emulator")] public bool IsEmulator { get; set; }
        [JsonProperty("is_running")] public bool IsRunning { get; set; }
        [JsonProperty("is_mobile")] public bool IsMobile { get; set; }


        [JsonIgnore] public bool IsAndroid => Platform.Equals(Platforms.Android);
        [JsonIgnore] public bool IsIPhone => Platform.Equals(Platforms.iOS);
        [JsonIgnore] public bool IsMacCatalyst => Platform.Equals(Platforms.MacCatalyst);
        [JsonIgnore] public bool IsWindows => Platform.Equals(Platforms.Windows);
    }
}
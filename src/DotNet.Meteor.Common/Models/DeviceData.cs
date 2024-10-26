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

public static class Details {
    public const string AndroidEmulator = "Android Emulators";
    public const string AndroidDevice = "Android Devices";
    public const string iOSSimulator = "iOS Simulators";
    public const string iOSDevice = "iOS Devices";
    public const string MacCatalyst = "Mac";
    public const string Windows = "Windows";

    public const string MacArm = "Apple Silicon";
    public const string MacX64 = "Intel";
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
    public const string WindowsX64 = "win-x64";
    public const string WindowsArm64 = "win-arm64";
    public const string iOSSimulatorX64 = "iossimulator-x64";
    public const string iOSSimulatorArm64 = "iossimulator-arm64";
}

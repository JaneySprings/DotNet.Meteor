using System.Text.Json.Serialization;

namespace DotNet.Meteor.Workspace.Devices;

public class DeviceData {
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("platform")] public string Platform { get; set; }
    [JsonPropertyName("serial")] public string? Serial { get; set; }
    [JsonPropertyName("runtime_id")] public string? RuntimeId { get; set; }
    [JsonPropertyName("os_version")] public string? OSVersion { get; set; }
    [JsonPropertyName("is_emulator")] public bool IsEmulator { get; set; }
    [JsonPropertyName("is_running")] public bool IsRunning { get; set; }
    [JsonPropertyName("is_mobile")] public bool IsMobile { get; set; }

    public DeviceData(string name, string platform) {
        Name = name;
        Platform = platform;
    }
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

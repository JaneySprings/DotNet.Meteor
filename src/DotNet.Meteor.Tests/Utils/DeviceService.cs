using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Tests;

public static class DeviceService {
    public const string Android = "android";
    public const string MacArm64 = "mac-arm64";
    public const string MacX64 = "mac-x64";
    public const string AppleSimulatorX64 = "ios-x64";
    public const string AppleArm64 = "ios-arm64";
    public const string Windows10 = "windows10.0.19041.0";

    private readonly static Dictionary<string, DeviceData> _devices = new Dictionary<string, DeviceData>() {
        { MacArm64, new DeviceData { Platform = Platforms.MacCatalyst, RuntimeId = Runtimes.MacArm64 } },
        { MacX64, new DeviceData { Platform = Platforms.MacCatalyst, RuntimeId = Runtimes.MacX64 } },
        { AppleSimulatorX64, new DeviceData { Platform = Platforms.iOS, RuntimeId = Runtimes.iOSSimulatorX64 } },
        { AppleArm64, new DeviceData { Platform = Platforms.iOS, RuntimeId = Runtimes.iOSArm64 } },
        { Windows10, new DeviceData { Platform = Platforms.Windows, RuntimeId = Windows10 } },
        { Android, new DeviceData { Platform = Platforms.Android } },
    };

    public static DeviceData? GetDevice(string id) {
        if (_devices.TryGetValue(id, out var device))
            return device;
        return null;
    }
}
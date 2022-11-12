using System;
using System.Linq;
using DotNet.Mobile.Shared;

namespace Android.Sdk {
    public class PhysicalDevice {
        public string Serial { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string Product { get; set; }
        public string Model { get; set; }
        public string Device { get; set; }
        public string TransportId { get; set; }
    }

    public static class PhysicalDeviceExtensions {
        public static string GetAVDName(this PhysicalDevice device) {
            var adb = PathUtils.GetADBTool();
            ProcessResult result = ProcessRunner.Run(adb, new ProcessArgumentBuilder()
                .Append("-s", device.Serial)
                .Append("emu", "avd", "name")
            );

            if (result.ExitCode != 0)
                Logger.Error(string.Join(Environment.NewLine, result.StandardError));

            return result.StandardOutput.FirstOrDefault();
        }

        public static DeviceData ToDeviceData(this PhysicalDevice device) {
            bool isEmulator = device.Serial.Contains("emulator");
            var result = new DeviceData {
                Serial = device.Serial,
                Platform = Platform.Android,
                IsRunning = true,
                IsEmulator = isEmulator,
                RuntimeIdentifier = "android-arm64"
            };

            if (isEmulator) {
                result.Name = device.GetAVDName();
                result.Details = "Emulator";
            } else {
                result.Name = device.Serial;
                result.Details = "Physical Device";
            }

            return result;
        }
    }
}
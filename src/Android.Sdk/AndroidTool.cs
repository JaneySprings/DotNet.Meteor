using DotNet.Mobile.Shared;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Android.Sdk {
    public static class AndroidTool {
        public static List<DeviceData> VirtualDevicesFast() {
            List<DeviceData> avds = new List<DeviceData>();
            string avdHome = PathUtils.AvdLocation();

            foreach (var file in Directory.GetFiles(avdHome, "*.ini")) {
                var ini = new IniFile(file);
                avds.Add(new DeviceData {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Details = "Emulator",
                    Platform = Platform.Android,
                    OSVersion = ini.GetField("target") ?? "Unknown",
                    IsEmulator = true,
                    IsRunning = false
                });
                ini.Free();
            }
            return avds;
        }

        public static List<DeviceData> AllDevices() {
            var virtualDevices = VirtualDevicesFast();
            var runningDevices = DeviceBridge.Devices();
            var allDevices = new List<DeviceData>();

            foreach(var serial in runningDevices) {
                if (serial.StartsWith("emulator-")) {
                    var name = DeviceBridge.EmuName(serial);
                    var avd = virtualDevices.Find(x => x.Name.Equals(name));

                    if (avd == null)
                        continue;

                    virtualDevices.Remove(avd);
                    avd.Serial = serial;
                    avd.IsRunning = true;
                    allDevices.Add(avd);
                } else {
                    allDevices.Add(new DeviceData {
                        Name = DeviceBridge.Shell(serial, "getprop", "ro.product.model"),
                        OSVersion = $"android-{DeviceBridge.Shell(serial, "getprop", "ro.build.version.sdk")}",
                        Platform = Platform.Android,
                        Details = "Device",
                        IsEmulator = false,
                        IsRunning = true,
                        Serial = serial
                    });
                }
            }

            allDevices.AddRange(virtualDevices.OrderBy(x => x.Name));
            return allDevices;
        }
    }
}
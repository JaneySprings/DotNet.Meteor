using DotNet.Mobile.Shared;
using System.Collections.Generic;
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

            foreach(var device in runningDevices) {
                if (device.IsEmulator) {
                    var name = DeviceBridge.EmuName(device.Serial);
                    var avd = virtualDevices.Find(x => x.Name.Equals(name));

                    if (avd == null)
                        continue;

                    virtualDevices.Remove(avd);
                    avd.Serial = device.Serial;
                    avd.IsRunning = true;
                    allDevices.Add(avd);
                } else {
                    allDevices.Add(device);
                }
            }

            allDevices.AddRange(virtualDevices);
            return allDevices;
        }
    }
}
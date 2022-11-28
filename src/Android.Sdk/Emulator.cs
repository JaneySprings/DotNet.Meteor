using System;
using System.IO;
using System.Linq;
using System.Threading;
using DotNet.Mobile.Shared;

namespace Android.Sdk {
    public static class Emulator {
        private const int AppearingRetryCount = 15;
        private const int SyncRetryCount = 300;

        public static FileInfo ToolLocation() {
            string sdk = PathUtils.GetSdkLocation();
            string path = Path.Combine(sdk, "emulator", "emulator");

            if (!File.Exists(path))
                throw new Exception("Could not find emulator tool");

            return new FileInfo(path);
        }

        public static string Run(string name) {
            var emulator = Emulator.ToolLocation();
            var process = new ProcessRunner(emulator, new ProcessArgumentBuilder()
                .Append("-avd")
                .Append(name));

            process.WaitForExitAsync();
            return Emulator.WaitForBoot();
        }

        public static string WaitForBoot() {
            string serial = WaitForSerial();

            if (serial == null)
                throw new Exception("Emulator started but no serial number was found");

            for (int i = 0; i < SyncRetryCount; i++) {
                Thread.Sleep(1000);

                if (DeviceBridge.Shell(serial, "getprop", "sys.boot_completed").Contains("1"))
                    return serial;
            }

            return null;
        }

        private static string WaitForSerial() {
            var currentState = DeviceBridge.Devices();

            for (int i = 0; i < AppearingRetryCount; i++) {
                Thread.Sleep(1000);
                var newState = DeviceBridge.Devices();

                if (newState.Count > currentState.Count) {
                    var newDevice = newState.Find(d => !currentState.Any(c => c.Serial == d.Serial));
                    if (newDevice != null)
                        return newDevice.Serial;
                }
            }
            return null;
        }
    }
}
using System;
using System.Linq;
using System.Threading;

namespace Android.Sdk {
    public static class Emulator {
        private const int AppearingRetryCount = 15;
        private const int SyncRetryCount = 300;

        public static string WaitForBoot() {
            string serial = WaitForSerial();

            if (serial == null)
                throw new Exception("Emulator started but no serial number was found");

            for (int i = 0; i < SyncRetryCount; i++) {
                Thread.Sleep(1000);

                if (AndroidTool.AdbShell(serial, "getprop", "sys.boot_completed").Contains("1"))
                    return serial;
            }

            return null;
        }

        private static string WaitForSerial() {
            var currentState = AndroidTool.GetActiveDevices();

            for (int i = 0; i < AppearingRetryCount; i++) {
                Thread.Sleep(1000);
                var newState = AndroidTool.GetActiveDevices();

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
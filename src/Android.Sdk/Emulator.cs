using System;
using System.Linq;
using System.Threading;
using DotNet.Meteor.Shared;

namespace Android.Sdk {
    public static class Emulator {
        private const int AppearingRetryCount = 120; //seconds

        public static string Run(string name) {
            var serial = SerialIfRunning(name);
            if (serial != null)
                return serial;

            var emulator = PathUtils.EmulatorTool();
            var process = new ProcessRunner(emulator, new ProcessArgumentBuilder()
                .Append("-avd")
                .Append(name));

            process.Start();
            return Emulator.WaitForBoot();
        }

        public static string WaitForBoot() {
            string serial = WaitForSerial();

            if (serial == null)
                throw new Exception("Emulator started but no serial number was found");

            while (!DeviceBridge.Shell(serial, "getprop", "sys.boot_completed").Contains("1"))
                Thread.Sleep(1000);

            return serial;
        }

        private static string WaitForSerial() {
            var currentState = DeviceBridge.Devices();

            for (int i = 0; i < AppearingRetryCount; i++) {
                Thread.Sleep(1000);
                var newState = DeviceBridge.Devices();

                if (newState.Count > currentState.Count) {
                    var newSerial = newState.Find(n => !currentState.Any(o => n.Equals(o)));
                    if (newSerial != null)
                        return newSerial;
                }
            }
            return null;
        }

        private static string SerialIfRunning(string avdName) {
            var serials = DeviceBridge.Devices().Where(it => it.StartsWith("emulator-"));
            return serials.FirstOrDefault(it => DeviceBridge.EmuName(it).Equals(avdName));
        }
    }
}
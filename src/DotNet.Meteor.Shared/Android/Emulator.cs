using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using DotNet.Meteor.Processes;

namespace DotNet.Meteor.Android {
    public static class Emulator {
        private const int AppearingRetryCount = 120; //seconds

        public static StartResult Run(string name) {
            var rSerial = SerialIfRunning(name);
            if (rSerial != null)
                return new StartResult(rSerial, null);

            var emulator = PathUtils.EmulatorTool();
            var runner = new ProcessRunner(emulator, new ProcessArgumentBuilder()
                .Append("-avd")
                .Append(name));

            var process = runner.Start();
            var serial = Emulator.WaitForBoot();

            return new StartResult(serial, process);
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

        public class StartResult {
            public string Serial { get; }
            public Process Process { get; }

            public StartResult(string serial, Process process) {
                Serial = serial;
                Process = process;
            }
        }
    }
}
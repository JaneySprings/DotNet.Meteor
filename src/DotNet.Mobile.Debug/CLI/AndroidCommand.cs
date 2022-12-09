using System;
using System.Collections.Generic;
using System.Text.Json;
using Android.Sdk;
using DotNet.Mobile.Shared;

namespace DotNet.Mobile.Debug.CLI {
    public static class AndroidCommand {
        public static void AndroidDevicesAsJson(string[] args) {
            List<DeviceData> devices = AndroidDevices();
            Console.WriteLine(JsonSerializer.Serialize(devices));
        }

        public static List<DeviceData> AndroidDevices() {
            return AndroidTool.AllDevices();
        }

        public static void RunEmulator(string[] args) {
            if (args.Length < 2)
                throw new Exception ($"Missing parameter: {Program.CommandHandler[args[0]].Item1[1]}");
            string serial = Emulator.Run(args[1]);
            Console.WriteLine(JsonSerializer.Serialize(serial));
        }
        public static void AndroidSdkPath(string[] args) {
            string path = PathUtils.SdkLocation();
            Console.WriteLine(JsonSerializer.Serialize(path));
        }
    }
}

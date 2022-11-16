using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Android.Sdk;
using XCode.Sdk;
using DotNet.Mobile.Shared;

namespace DotNet.Mobile.Debug.CLI {
    public static class Command {
        public static void Help() {
            Console.WriteLine($"DotNet.Mobile.Debug.CLI version {Program.Version} for Visual Studio Code");
            Console.WriteLine("Copyright (C) Nikita Romanov. All rights reserved.");
            Console.WriteLine("\n\nCommands:\n");
            Console.WriteLine(" --android-devices");
            Console.WriteLine("\t\t\tList of all available Android devices");
            Console.WriteLine(" --apple-devices");
            Console.WriteLine("\t\t\tList of all available Apple devices");
            Console.WriteLine(" --devices");
            Console.WriteLine("\t\t\tList of all available devices");
            Console.WriteLine(" --run-emulator <avd-name>");
            Console.WriteLine("\t\t\tRun Android emulator");
            Console.WriteLine(" --free-port");
            Console.WriteLine("\t\t\tFind first available port");
            Console.WriteLine(" --start-session");
            Console.WriteLine("\t\t\tLaunch mono debugger session");
            Console.WriteLine(" --help");
            Console.WriteLine("\t\t\tShow this help");
        }
        public static void Version() {
            Console.WriteLine(Program.Version);
        }
        public static void Error(string parameter) {
            Console.WriteLine($"Unknown parameter: {parameter}");
        }

        public static void AndroidDevices() {
            List<DeviceData> devices = AndroidTool.GetAllDevices();
            Console.WriteLine(JsonSerializer.Serialize(devices));
        }
        public static void AppleDevices() {
            List<DeviceData> devices = XCodeTool.GetAllDevices();
            Console.WriteLine(JsonSerializer.Serialize(devices));
        }
        public static void AllDevices() {
            List<DeviceData> devices = AndroidTool.GetAllDevices().Concat(XCodeTool.GetAllDevices()).ToList();
            Console.WriteLine(JsonSerializer.Serialize(devices));
        }

        public static void RunEmulator(string[] args) {
            if (args.Length < 2)
                throw new Exception ("Missing parameter: <avd-name>");
            string serial = AndroidTool.RunEmulator(args[1]);
            Console.WriteLine(serial);
        }

        public static void FreePort() {
            int port = Utilities.FindFreePort();
            Console.WriteLine(port);
        }

        public static void StartSession() {
            Console.WriteLine("Starting Mono debugger session...");
            var debugSession = new MonoDebugSession();
            debugSession.Start(Console.OpenStandardInput(), Console.OpenStandardOutput()).Wait();
        }
    }
}

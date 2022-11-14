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
            Console.WriteLine("\nCommands:");
            Console.WriteLine(" --android-devices    List of all available Android devices");
            Console.WriteLine(" --apple-devices      List of all available Apple devices");
            Console.WriteLine(" --devices            List of all available devices");
            Console.WriteLine(" --start-session      Launch mono debugger session");
            Console.WriteLine(" --help               Show this help");
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

        public static void StartSession() {
            Console.WriteLine("Starting Mono debugger session...");
            var debugSession = new MonoDebugSession {
                TRACE = true,
                TRACE_RESPONSE = true
            };
            debugSession.Start(Console.OpenStandardInput(), Console.OpenStandardOutput()).Wait();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Android.Sdk;
using XCode.Sdk;
using DotNet.Mobile.Shared;
using DotNet.Mobile.Debug.Session;

namespace DotNet.Mobile.Debug.CLI {
    public static class ConsoleUtils {
        public static void Help(string[] args) {
            Console.WriteLine($"DotNet.Mobile.Debug.CLI version {Program.Version} for Visual Studio Code");
            Console.WriteLine("Copyright (C) Nikita Romanov. All rights reserved.");
            Console.WriteLine("\nCommands:");

            foreach (var command in Program.CommandHandler) {
                Console.WriteLine("  {0,-30} {1,5}", 
                    command.Key + " " + string.Join(" ", command.Value.Item1.Skip(1)),
                    command.Value.Item1[0]
                );
            }
        }
        public static void Version(string[] args) {
            Console.WriteLine(Program.Version);
        }
        public static void Error(string[] args) {
            Console.WriteLine($"Unknown parameter: {args[0]}");
        }

        public static void AndroidDevices(string[] args) {
            List<DeviceData> devices = AndroidTool.GetAllDevices();
            Console.WriteLine(JsonSerializer.Serialize(devices));
        }
        public static void AppleDevices(string[] args) {
            List<DeviceData> devices = XCodeTool.GetAllDevices();
            Console.WriteLine(JsonSerializer.Serialize(devices));
        }

        public static void RunEmulator(string[] args) {
            if (args.Length < 2)
                throw new Exception ($"Missing parameter: {Program.CommandHandler[args[0]].Item1[1]}");
            string serial = AndroidTool.RunEmulator(args[1]);
            Console.WriteLine(serial);
        }

        public static void FindProjects(string[] args) {
            if (args.Length < 2)
                throw new Exception ($"Missing parameter: {Program.CommandHandler[args[0]].Item1[1]}");
            List<Project> projects = WorkspaceAnalyzer.GetProjects(args[1]);
            Console.WriteLine(JsonSerializer.Serialize(projects));
        }

        public static void FreePort(string[] args) {
            int port = Utilities.FindFreePort();
            Console.WriteLine(port);
        }

        public static void StartSession(string[] args) {
            Console.WriteLine("Starting Mono debugger session...");
            var debugSession = new MonoDebugSession();
            debugSession.Start(Console.OpenStandardInput(), Console.OpenStandardOutput()).Wait();
        }
    }
}

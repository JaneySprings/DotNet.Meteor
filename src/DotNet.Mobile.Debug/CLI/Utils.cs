using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Android.Sdk;
using XCode.Sdk;
using DotNet.Mobile.Shared;
using DotNet.Mobile.Debug.Session;
using Platform = DotNet.Mobile.Shared.Platform;

namespace DotNet.Mobile.Debug.CLI {
    public static class ConsoleUtils {
        public static void Help(string[] args) {
            Console.WriteLine($"DotNet.Mobile.Debug.CLI version {Program.Version} for Visual Studio Code");
            Console.WriteLine("Copyright (C) Nikita Romanov. All rights reserved.");
            Console.WriteLine("\nCommands:");

            foreach (var command in Program.CommandHandler) {
                Console.WriteLine("  {0,-40} {1,5}",
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
            List<DeviceData> devices = AndroidTool.AllDevices();
            Console.WriteLine(JsonSerializer.Serialize(devices));
        }
        public static void AppleDevices(string[] args) {
            List<DeviceData> devices = XCodeTool.AllDevices();
            Console.WriteLine(JsonSerializer.Serialize(devices));
        }
        public static void AllDevices(string[] args) {
            var devices = new List<DeviceData>();
            devices.AddRange(AndroidTool.AllDevices());
            devices.AddRange(XCodeTool.AllDevices());
            Console.WriteLine(JsonSerializer.Serialize(devices));
        }
        public static void DeviceInfo(string[] args) {
            if (args.Length < 2)
                throw new Exception ($"Missing parameter: {Program.CommandHandler[args[0]].Item1[1]} {Program.CommandHandler[args[0]].Item1[2]}");
            if (args.Length < 3)
                throw new Exception ($"Missing parameter: {Program.CommandHandler[args[0]].Item1[2]}");

            var devices = new List<DeviceData>();

            if (args[1].Equals(Platform.Android))
                devices.AddRange(AndroidTool.AllDevices());
            else if (args[1].Equals(Platform.iOS))
                devices.AddRange(XCodeTool.AllDevices());

            var device = devices.Find(d => d.Name.Equals(args[2]));
            device ??= new DeviceData();

            Console.WriteLine(JsonSerializer.Serialize(device));
        }

        public static void AnalyzeWorkspace(string[] args) {
            if (args.Length < 2)
                throw new Exception ($"Missing parameter: {Program.CommandHandler[args[0]].Item1[1]}");
            List<Project> projects = WorkspaceAnalyzer.AnalyzeWorkspace(args[1]);
            Console.WriteLine(JsonSerializer.Serialize(projects));
        }
        public static void AnalyzeProject(string[] args) {
            if (args.Length < 2)
                throw new Exception ($"Missing parameter: {Program.CommandHandler[args[0]].Item1[1]}");
            Project project = WorkspaceAnalyzer.AnalyzeProject(args[1]);
            Console.WriteLine(JsonSerializer.Serialize(project));
        }


        public static void RunEmulator(string[] args) {
            if (args.Length < 2)
                throw new Exception ($"Missing parameter: {Program.CommandHandler[args[0]].Item1[1]}");
            string serial = Emulator.Run(args[1]);
            Console.WriteLine(JsonSerializer.Serialize(serial));
        }
        public static void AndroidSdkPath(string[] args) {
            string path = Android.Sdk.PathUtils.SdkLocation();
            Console.WriteLine(JsonSerializer.Serialize(path));
        }
        public static void FreePort(string[] args) {
            int port = Utilities.FindFreePort();
            Console.WriteLine(JsonSerializer.Serialize(port));
        }
        public static void StartSession(string[] args) {
            Console.WriteLine("Starting Mono debugger session...");
            var debugSession = new MonoDebugSession();
            debugSession.Start(Console.OpenStandardInput(), Console.OpenStandardOutput()).Wait();
        }
    }
}

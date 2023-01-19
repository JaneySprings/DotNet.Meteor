using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DotNet.Meteor.Shared;
using DotNet.Meteor.Debug.Session;
using DotNet.Meteor.Apple;
using DotNet.Meteor.Android;

namespace DotNet.Meteor.Debug.CLI {
    public static class ConsoleUtils {
        public static void Help(string[] args) {
            Console.WriteLine($"DotNet.Meteor.Debug.CLI version {Program.Version} for Visual Studio Code");
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


        public static void AllDevices(string[] args) {
            var devices = DeviceProvider.GetDevices();
            Console.WriteLine(JsonSerializer.Serialize(devices));
        }

        public static void AndroidSdkPath(string[] args) {
            string path = DotNet.Meteor.Android.PathUtils.SdkLocation();
            Console.WriteLine(JsonSerializer.Serialize(path));
        }

        public static void AnalyzeWorkspace(string[] args) {
            var projects = new List<Project>();
            if (args.Length < 2)
                throw new Exception ($"Missing parameter: {Program.CommandHandler[args[0]].Item1[1]}");

            for (int i = 1; i < args.Length; i++)
                projects.AddRange(WorkspaceAnalyzer.AnalyzeWorkspace(args[i]));

            Console.WriteLine(JsonSerializer.Serialize(projects));
        }

        public static void AnalyzeProject(string[] args) {
            if (args.Length < 2)
                throw new Exception ($"Missing parameter: {Program.CommandHandler[args[0]].Item1[1]}");
            Project project = WorkspaceAnalyzer.AnalyzeProject(args[1]);
            Console.WriteLine(JsonSerializer.Serialize(project));
        }

        public static void StartSession(string[] args) {
            Console.WriteLine("Starting Mono debugger session...");
            var debugSession = new MonoDebugSession();
            debugSession.Start(Console.OpenStandardInput(), Console.OpenStandardOutput()).Wait();
        }
    }
}

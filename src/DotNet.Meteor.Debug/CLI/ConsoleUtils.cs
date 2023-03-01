using System;
using System.Collections.Generic;
using System.Text.Json;
using DotNet.Meteor.Shared;
using DotNet.Meteor.Xaml;
using System.Reflection;
using NLog;

namespace DotNet.Meteor.Debug.CLI {
    public static class ConsoleUtils {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public static void Help() {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"DotNet.Meteor.Debug.CLI version {version?.Major}.{version?.Minor}.{version?.Build} for Visual Studio Code");
            Console.WriteLine("Copyright (C) Nikita Romanov. All rights reserved.");
            Console.WriteLine("\nCommands:");

            foreach (var command in Program.CommandHandler.Keys)
                Console.WriteLine($" {command}");
        }


        public static void AllDevices(string[] args) {
            var devices = DeviceProvider.GetDevices(logger.Error);
            Console.WriteLine(JsonSerializer.Serialize(devices));
        }

        public static void AndroidSdkPath(string[] args) {
            string path = Android.PathUtils.SdkLocation(logger.Error);
            Console.WriteLine(JsonSerializer.Serialize(path));
        }

        public static void AnalyzeWorkspace(string[] args) {
            var projects = new List<Project>();

            for (int i = 1; i < args.Length; i++)
                projects.AddRange(WorkspaceAnalyzer.AnalyzeWorkspace(args[i], logger.Debug));

            Console.WriteLine(JsonSerializer.Serialize(projects));
        }

        public static void XamlGenerate(string[] args) {
            var schemaGenerator = new JsonSchemaGenerator(args[1], logger.Error);
            var result = schemaGenerator.CreateTypesAlias();
            Console.WriteLine(JsonSerializer.Serialize(result));
        }

        public static void StartSession(string[] args) {
            Console.WriteLine("Starting Mono debugger session...");
            var debugSession = new DebugSession();
            debugSession.Start(Console.OpenStandardInput(), Console.OpenStandardOutput()).Wait();
        }
    }
}

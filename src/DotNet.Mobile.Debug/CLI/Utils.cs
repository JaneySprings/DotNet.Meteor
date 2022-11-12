using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Android.Sdk;
using XCode.Sdk;
using DotNet.Mobile.Shared;

namespace DotNet.Mobile.Debug.CLI {
    public static class Utils {
        public static void CommandHelp() {
            Logger.Info($"DotNet.Mobile.Debug.CLI version {Program.Version} for Visual Studio Code");
            Logger.Info("Copyright (C) Nikita Romanov. All rights reserved.");
            Logger.Info("\nCommands:");
            Logger.Info(" --android-devices    List of all available Android devices");
            Logger.Info(" --apple-devices      List of all available Apple devices");
            Logger.Info(" --devices            List of all available devices");
            Logger.Info(" --start-session      Launch mono debugger session");
            Logger.Info(" --help               Show this help");
        }
        public static void CommandVersion() {
            Logger.Info(Program.Version);
        }
        public static void CommandError(string parameter) {
            Logger.Error($"Unknown parameter: {parameter}");
        }

        public static void CommandAndroidDevices() {
            List<DeviceData> devices = AndroidTool.GetAllDevices();
            Logger.Info(JsonSerializer.Serialize(devices));
        }
        public static void CommandAppleDevices() {
            List<DeviceData> devices = XCodeTool.GetAllDevices();
            Logger.Info(JsonSerializer.Serialize(devices));
        }
        public static void CommandAllDevices() {
            List<DeviceData> devices = AndroidTool.GetAllDevices().Concat(XCodeTool.GetAllDevices()).ToList();
            Logger.Info(JsonSerializer.Serialize(devices));
        }

        public static void CommandStartSession() {
            Logger.Info("Starting Mono debugger session...");
            // var debugSession = new MonoDebugSession {
            //     TRACE = true,
            //     TRACE_RESPONSE = true
            // };
            // debugSession.Start(Console.OpenStandardInput(), Console.OpenStandardOutput()).Wait();
        }
    }
}

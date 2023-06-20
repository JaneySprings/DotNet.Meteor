using System;
using System.Collections.Generic;
using DotNet.Meteor.HotReload;
using DotNet.Meteor.Logging;
using DotNet.Meteor.Shared;
using DotNet.Meteor.Debug;
using DotNet.Meteor.Xaml;
using System.Reflection;
using Newtonsoft.Json;

namespace DotNet.Meteor.CommandLine;

public class Program {
    public static readonly Dictionary<string, Action<string[]>> CommandHandler = new() {
        {  "--all-devices", AllDevices },
        { "--android-sdk-path", AndroidSdkPath },
        { "--analyze-workspace", AnalyzeWorkspace },
        { "--xaml-reload", XamlReload },
        { "--xaml", XamlGenerate },
        { "--start-session", StartSession }
    };

    private static void Main(string[] args) {
        LogConfig.InitializeLog();
        if (args.Length == 0) {
            Help();
            return;
        }

        if (CommandHandler.TryGetValue(args[0], out var command))
            command.Invoke(args);
    }

    public static void Help() {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var name = Assembly.GetExecutingAssembly().GetName().Name;
        Console.WriteLine($"{name} version {version?.Major}.{version?.Minor}.{version?.Build} for Visual Studio Code");
        Console.WriteLine("Copyright (C) Nikita Romanov. All rights reserved.");
        Console.WriteLine("\nCommands:");

        foreach (var command in Program.CommandHandler.Keys)
            Console.WriteLine($" {command}");
    }

    public static void AllDevices(string[] args) {
        var devices = DeviceProvider.GetDevices();
        Console.WriteLine(JsonConvert.SerializeObject(devices));
    }

    public static void AndroidSdkPath(string[] args) {
        string path = Android.PathUtils.SdkLocation();
        Console.WriteLine(JsonConvert.SerializeObject(path));
    }

    public static void AnalyzeWorkspace(string[] args) {
        var projects = new List<Project>();

        for (int i = 1; i < args.Length; i++)
            projects.AddRange(WorkspaceAnalyzer.AnalyzeWorkspace(args[i]));

        Console.WriteLine(JsonConvert.SerializeObject(projects));
    }

    public static void XamlGenerate(string[] args) {
        var schemaGenerator = new JsonSchemaGenerator(args[1]);
        var result = schemaGenerator.CreateTypesAlias();
        Console.WriteLine(JsonConvert.SerializeObject(result));
    }

    public static void XamlReload(string[] args) {
        var result = HotReloadClient.SendNotification(int.Parse(args[1]), args[2]);
        Console.WriteLine(JsonConvert.SerializeObject(result));
    }

    public static void StartSession(string[] args) {
        var debugSession = new DebugSession(Console.OpenStandardInput(), Console.OpenStandardOutput());
        debugSession.Start();
    }
}

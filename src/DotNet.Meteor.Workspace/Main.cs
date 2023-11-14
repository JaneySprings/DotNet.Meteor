using DotNet.Meteor.Debug;
using DotNet.Meteor.HotReload;
using DotNet.Meteor.Shared;
using DotNet.Meteor.Xaml;
using System.Reflection;
using System.Text.Json;

namespace DotNet.Meteor.Workspace;

public class Program {
    public static readonly Dictionary<string, Action<string[]>> CommandHandler = new() {
        {  "--all-devices", AllDevices },
        { "--android-sdk-path", AndroidSdkPath },
        { "--analyze-workspace", AnalyzeWorkspace },
        { "--xaml-reload", XamlReload },
        { "--xaml", XamlGenerate },
        { "--help", Help }
    };

    private static void Main(string[] args) {
        if (args.Length == 0) {
            StartDebug(args);
            return;
        }

        if (CommandHandler.TryGetValue(args[0], out var command))
            command.Invoke(args);
    }

    public static void Help(string[] args) {
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
        Console.WriteLine(JsonSerializer.Serialize(devices, TrimmableContext.Default.ListDeviceData));
    }

    public static void AndroidSdkPath(string[] args) {
        string path = AndroidUtilities.SdkLocation();
        Console.WriteLine(path);
    }

    public static void AnalyzeWorkspace(string[] args) {
        var projects = new List<Project>();

        for (int i = 1; i < args.Length; i++)
            projects.AddRange(WorkspaceAnalyzer.AnalyzeWorkspace(args[i]));

        Console.WriteLine(JsonSerializer.Serialize(projects, TrimmableContext.Default.ListProject));
    }

    public static void XamlGenerate(string[] args) {
        var schemaGenerator = new JsonSchemaGenerator(args[1]);
        var result = schemaGenerator.CreateTypesAlias();
        Console.WriteLine(JsonSerializer.Serialize(result, TrimmableContext.Default.Boolean));
    }

    public static void XamlReload(string[] args) {
        var result = HotReloadClient.SendNotification(int.Parse(args[1]), args[2]);
        Console.WriteLine(JsonSerializer.Serialize(result, TrimmableContext.Default.Boolean));
    }

    public static void StartDebug(string[] args) {
        var debugSession = new DebugSession(Console.OpenStandardInput(), Console.OpenStandardOutput());
        debugSession.Start();
    }
}

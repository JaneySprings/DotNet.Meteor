using DotNet.Meteor.Shared;
using NLog;
using System.Reflection;
using System.Text.Json;

namespace DotNet.Meteor.Workspace;

public class Program {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public static readonly Dictionary<string, Action<string[]>> CommandHandler = new() {
        { "--all-devices", AllDevices },
        { "--android-sdk-path", AndroidSdkPath },
        { "--analyze-workspace", AnalyzeWorkspace },
        { "--help", Help }
    };

    private static void Main(string[] args) {
        if (args.Length == 0) {
            Help(args);
            return;
        }

        LogConfig.InitializeLog();
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
        var devices = DeviceProvider.GetDevices(logger.Error, logger.Debug);
        Console.WriteLine(JsonSerializer.Serialize(devices, TrimmableContext.Default.ListDeviceData));
    }

    public static void AndroidSdkPath(string[] args) {
        string path = AndroidSdk.SdkLocation();
        Console.WriteLine(path);
    }

    public static void AnalyzeWorkspace(string[] args) {
        var projects = new List<Project>();

        for (int i = 1; i < args.Length; i++)
            projects.AddRange(WorkspaceAnalyzer.AnalyzeWorkspace(args[i], logger.Info));

        Console.WriteLine(JsonSerializer.Serialize(projects, TrimmableContext.Default.ListProject));
    }
}

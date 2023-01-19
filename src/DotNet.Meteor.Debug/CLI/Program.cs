using System;
using System.Reflection;
using System.Collections.Generic;

namespace DotNet.Meteor.Debug.CLI;

public class Program {
    public static string Version {
        get {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version?.Major}.{version?.Minor}.{version?.Build}";
        }
    }

    public static readonly Dictionary<string, Tuple<string[], Action<string[]>>> CommandHandler = new() {
        {
            "--all-devices", new Tuple<string[], Action<string[]>>(new []{
                "List of all available devices"
            }, ConsoleUtils.AllDevices)
        },
        {
            "--android-sdk-path", new Tuple<string[], Action<string[]>>(new []{
                "Get actual Android SDK path"
            }, ConsoleUtils.AndroidSdkPath)
        },
        {
            "--analyze-workspace", new Tuple<string[], Action<string[]>>(new []{
                "Find all executable projects in workspace", "<cwd-path>"
            }, ConsoleUtils.AnalyzeWorkspace)
        },
        {
            "--analyze-project", new Tuple<string[], Action<string[]>>(new []{
                "Get info about specified project", "<project-path>"
            }, ConsoleUtils.AnalyzeProject)
        },
        {
            "--start-session", new Tuple<string[], Action<string[]>>(new []{
                "Launch mono debugger session"
            }, ConsoleUtils.StartSession)
        },
        {
            "--version", new Tuple<string[], Action<string[]>>(new []{
                "Show tool version"
            }, ConsoleUtils.Version)
        },
        {
            "--help", new Tuple<string[], Action<string[]>>(new []{
                "Show this help"
            }, ConsoleUtils.Help)
        }
    };

    private static void Main(string[] args) {
        if (args.Length == 0) {
            ConsoleUtils.Help(args);
            return;
        }

        if (CommandHandler.TryGetValue(args[0], out var command)) {
            command.Item2.Invoke(args);
        } else {
            ConsoleUtils.Error(args);
        }
    }
}

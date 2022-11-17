using System;
using System.Reflection;
using System.Collections.Generic;

namespace DotNet.Mobile.Debug.CLI;

public class Program {
    public static string Version {
        get {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version?.Major}.{version?.Minor}.{version?.Build}";
        }
    }

    public static readonly Dictionary<string, Tuple<string[], Action<string[]>>> CommandHandler = new() {
        {
            "--android-devices", new Tuple<string[], Action<string[]>>(new []{
                "List of all available Android devices"
            }, ConsoleUtils.AndroidDevices)
        },
        {
            "--apple-devices", new Tuple<string[], Action<string[]>>(new []{
                "List of all available Apple devices"
            }, ConsoleUtils.AppleDevices)
        },
        {
            "--run-emulator", new Tuple<string[], Action<string[]>>(new []{
                "Run Android emulator", "<avd-name>"
            }, ConsoleUtils.RunEmulator)
        },
        {
            "--free-port", new Tuple<string[], Action<string[]>>(new []{
                "Find first available port"
            }, ConsoleUtils.FreePort)
        },
        {
            "--find-projects", new Tuple<string[], Action<string[]>>(new []{
                "Find all projects in workspace", "<cwd-path>"
            }, ConsoleUtils.FindProjects)
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

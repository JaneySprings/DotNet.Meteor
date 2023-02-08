using System;
using System.Collections.Generic;
using DotNet.Meteor.Logging;

namespace DotNet.Meteor.Debug.CLI;

public class Program {
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
            "--start-session", new Tuple<string[], Action<string[]>>(new []{
                "Launch mono debugger session"
            }, ConsoleUtils.StartSession)
        }
    };

    private static void Main(string[] args) {
        LogConfig.InitializeLog();
        if (args.Length == 0) {
            ConsoleUtils.Help(args);
            return;
        }

        if (CommandHandler.TryGetValue(args[0], out var command)) {
            command.Item2.Invoke(args);
        } else {
            ConsoleUtils.Help(args);
        }
    }
}

using System;
using System.Collections.Generic;
using DotNet.Meteor.Logging;

namespace DotNet.Meteor.Debug.CLI;

public class Program {
    public static readonly Dictionary<string, Action<string[]>> CommandHandler = new() {
        {  "--all-devices", ConsoleUtils.AllDevices },
        { "--android-sdk-path", ConsoleUtils.AndroidSdkPath },
        { "--analyze-workspace", ConsoleUtils.AnalyzeWorkspace },
        { "--xaml", ConsoleUtils.XamlGenerate },
        { "--start-session", ConsoleUtils.StartSession }
    };

    private static void Main(string[] args) {
        LogConfig.InitializeLog();
        if (args.Length == 0) {
            ConsoleUtils.Help();
            return;
        }

        if (CommandHandler.TryGetValue(args[0], out var command))
            command.Invoke(args);
    }
}

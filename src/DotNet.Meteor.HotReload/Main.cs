using DotNet.Meteor.Shared;
using NLog;
using System.Reflection;
using System.Text.Json;

namespace DotNet.Meteor.HotReload;

public class Program {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    internal static string GetVersion() {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version?.ToString() ?? string.Empty;
    }

    private static void Main(string[] args) {
        if (args.Length == 0) {
            Help(args);
            return;
        }

        LogConfig.InitializeLog();
        var result = HotReloadClient.SendNotification(int.Parse(args[0]), args[1], logger.Error);
        Console.WriteLine(JsonSerializer.Serialize(result, TrimmableContext.Default.Boolean));
    }

    public static void Help(string[] args) {
        var name = Assembly.GetExecutingAssembly().GetName().Name;
        Console.WriteLine($"{name} version {GetVersion()} for Visual Studio Code");
        Console.WriteLine("Copyright (C) Nikita Romanov. All rights reserved.");
    }
}

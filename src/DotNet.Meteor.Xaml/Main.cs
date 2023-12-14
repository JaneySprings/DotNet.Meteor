using DotNet.Meteor.Shared;
using DotNet.Meteor.Xaml.HotReload;
using NLog;
using System.Reflection;
using System.Text.Json;

namespace DotNet.Meteor.Xaml;

public class Program {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public static readonly Dictionary<string, Action<string[]>> CommandHandler = new() {
        { "--xaml", XamlGenerate },
        { "--xaml-reload", XamlReload },
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

    public static void XamlGenerate(string[] args) {
        var schemaGenerator = new JsonSchemaGenerator(args[1], logger.Error);
        var result = schemaGenerator.CreateTypesAlias();
        Console.WriteLine(JsonSerializer.Serialize(result, TrimmableContext.Default.Boolean));
    }

    public static void XamlReload(string[] args) {
        var result = HotReloadClient.SendNotification(int.Parse(args[1]), args[2], logger.Error);
        Console.WriteLine(JsonSerializer.Serialize(result, TrimmableContext.Default.Boolean));
    }
}

using DotNet.Meteor.Shared;
using NLog;
using System.Reflection;
using System.Text.Json;

namespace DotNet.Meteor.Xaml;

public class Program {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private static void Main(string[] args) {
        if (args.Length == 0) {
            Help(args);
            return;
        }

        LogConfig.InitializeLog();
        var schemaGenerator = new JsonSchemaGenerator(args[0], logger.Error);
        var result = schemaGenerator.CreateTypesAlias();
        Console.WriteLine(JsonSerializer.Serialize(result, TrimmableContext.Default.Boolean));
    }

    public static void Help(string[] args) {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var name = Assembly.GetExecutingAssembly().GetName().Name;
        Console.WriteLine($"{name} version {version?.Major}.{version?.Minor}.{version?.Build} for Visual Studio Code");
        Console.WriteLine("Copyright (C) Nikita Romanov. All rights reserved.");
    }
}

using System.Text.Json;

namespace DotNet.Meteor.Xaml;

public class Program {
    public static void Main(string[] args) {
        // if (args.Length == 0) {
        //     Console.WriteLine("DotNet.Meteor.Xaml: generating XSD files for XAML.");
        //     Console.WriteLine("Use: DotNet.Meteor.Xaml.dll\n");
        //     Console.WriteLine("  {0,-30} {1,5}", "--assembly=<path>", "Specifies path to .dll file to process.");
        //     Console.WriteLine("  {0,-30} {1,5}", "--target-xmlns=<name>", "Specifies target namespace.");
        //     Console.WriteLine("  {0,-30} {1,5}", "--out-file=<path>", "Specifies path to output file scheme.");
        //     Console.WriteLine();
        //     //return;
        // }

        //dbg
        string assembliesDir = "/Users/nromanov/Work/Sandbox/mauiTemplate/bin/Debug/net7.0-ios/iossimulator-x64";
        var extractor = new AssemblyExtractor();
        var elements = new List<TypeInfo>();

        foreach(var assemblyPath in Directory.GetFiles(assembliesDir, "*.dll", SearchOption.TopDirectoryOnly))
            try { elements.AddRange(extractor.ProcessAssembly(assemblyPath)); } catch {};

        SaveSchema(elements);
    }

    public static void SaveSchema(object schema) {
        var json = JsonSerializer.Serialize(schema);
        using var stream = File.CreateText("/Users/nromanov/Work/MAUI.json");
        stream.WriteLine(json);
    }
}
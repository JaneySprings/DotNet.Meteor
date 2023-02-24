using System.Reflection;
using System.Text.Json;

namespace DotNet.Meteor.Xaml;

public class JsonSchemaGenerator {
    private readonly Action<string>? logger;
    private readonly string projectPath;

    public JsonSchemaGenerator(string projectPath, Action<string>? logger = null) {
        this.projectPath = projectPath;
        this.logger = logger;
    }


    public bool CreateTypesAlias() {
        var outputDirectory = Path.Combine(Path.GetDirectoryName(this.projectPath)!, ".meteor", "generated");
        var typeLoader = new MauiTypeLoader(this.projectPath, this.logger);
        var reflector = new Reflector(this.logger);

        if (!typeLoader.LoadComparedTypes())
            return false;

        if (Directory.Exists(outputDirectory))
            Directory.Delete(outputDirectory, true);

        Directory.CreateDirectory(outputDirectory);

        foreach (var assemblyPath in Directory.GetFiles(typeLoader.AssembliesDirectory!, "*.dll", SearchOption.TopDirectoryOnly)) {
            try {
                var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
                if (assemblyName.StartsWith("System.") || assemblyName.StartsWith("Xamarin.") || assemblyName.StartsWith("Mono."))
                    continue;

                var schema = reflector.CreateAlias(Assembly.LoadFrom(assemblyPath));
                if (schema.Types?.Any() == false)
                    continue;

                Save(schema, Path.Combine(outputDirectory, $"{assemblyName}.json"));
            } catch (Exception e) {
                this.logger?.Invoke(e.Message);
            }
        }

        return true;
    }

    private void Save(object schema, string outputFile) {
        var json = JsonSerializer.Serialize(schema);
        using var stream = File.CreateText(outputFile);
        stream.WriteLine(json);
    }
}
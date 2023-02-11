using System.Reflection;
using System.Text.Json;

namespace DotNet.Meteor.Xaml;

public class JsonSchemaGenerator {
    private readonly Action<string>? logger;
    private readonly string projectPath;
    private readonly string framework;
    private readonly string rid;
    private readonly string outputDirectory;

    public JsonSchemaGenerator(string projectPath, string outputDir, string framework, string rid, Action<string>? logger = null) {
        this.projectPath = projectPath;
        this.outputDirectory = outputDir;
        this.framework = framework;
        this.rid = rid;
        this.logger = logger;
    }


    public bool CreateTypesAlias() {
        var typeLoader = new MauiTypeLoader(projectPath, framework, rid, this.logger);
        var reflector = new Reflector(this.logger);

        if (!typeLoader.LoadComparedTypes())
            return false;

        if (Directory.Exists(this.outputDirectory))
            Directory.Delete(this.outputDirectory, true);

        Directory.CreateDirectory(this.outputDirectory);

        foreach (var assemblyPath in Directory.GetFiles(typeLoader.AssembliesDirecotry!, "*.dll", SearchOption.TopDirectoryOnly)) {
            try {
                var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
                if (assemblyName.StartsWith("System.") || assemblyName.StartsWith("Xamarin.") || assemblyName.StartsWith("Mono."))
                    continue;

                var schema = reflector.ParseAssembly(Assembly.LoadFrom(assemblyPath));
                if (schema.Count == 0)
                    continue;

                Save(schema, Path.Combine(this.outputDirectory, $"{assemblyName}.json"));
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
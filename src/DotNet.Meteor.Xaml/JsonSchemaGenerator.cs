using System.Text.Json;

namespace DotNet.Meteor.Xaml;

public class JsonSchemaGenerator {
    private readonly Action<string>? logger;
    private readonly MauiTypeLoader typeLoader;
    private readonly Reflector typeReflector;
    private readonly string projectPath;
    private readonly string outputDirectory;

    public JsonSchemaGenerator(string projectPath, Action<string>? logger = null) {
        this.projectPath = projectPath;
        this.logger = logger;
        this.typeReflector = new Reflector(this.logger);
        this.typeLoader = new MauiTypeLoader(this.projectPath, this.logger);
        this.outputDirectory = Path.Combine(Path.GetDirectoryName(this.projectPath)!, ".meteor", "generated");
    }

    public bool CreateTypesAlias() {
        if (!typeLoader.LoadComparedTypes())
            return false;

        if (Directory.Exists(outputDirectory))
            Directory.Delete(outputDirectory, true);

        Directory.CreateDirectory(outputDirectory);

        foreach (var assemblyPath in Directory.GetFiles(typeLoader.AssembliesDirectory!, "*.dll", SearchOption.TopDirectoryOnly)) {
            try {
                var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
                var outputFile = Path.Combine(outputDirectory, $"{assemblyName}.json");
                if (assemblyName.StartsWith("System.") || assemblyName.StartsWith("Xamarin.") || assemblyName.StartsWith("Mono."))
                    continue;

                var schema = this.typeReflector.CreateAlias(assemblyPath);
                if (schema.Types?.Any() == false)
                    continue;

                schema.TimeStamp = DateTime.Now.ToString();
                schema.Target = this.typeLoader.AssembliesDirectory;
                if (schema == null)
                    continue;

                WriteSchema(schema, outputFile);
            } catch (Exception e) {
                this.logger?.Invoke(e.Message);
            }
        }

        return true;
    }

    private void WriteSchema(SchemaInfo schema, string outputFile) {
        var json = JsonSerializer.Serialize(schema, TrimmableContext.Default.SchemaInfo);
        using var stream = File.CreateText(outputFile);
        stream.WriteLine(json);
    }
}
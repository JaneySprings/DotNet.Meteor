using System.Reflection;

namespace DotNet.Meteor.Xaml;

public class MauiTypeLoader {
    public static Type? BindableObject { get; private set; }
    public static Type? XmlnsDefinitionAttribute { get; private set; }
    public string? AssembliesDirectory { get; private set; }

    private readonly Action<string>? logger;
    private readonly string projectFilePath;

    private const string MAUI_CONTROLS_ASSEMBLY = "Microsoft.Maui.Controls.dll";
    private const string MAUI_BINDABLE_OBJECT = "Microsoft.Maui.Controls.BindableObject";
    private const string MAUI_XMLNS_DEFINITION = "Microsoft.Maui.Controls.XmlnsDefinitionAttribute";


    public MauiTypeLoader(string path, Action<string>? logger = null) {
        this.projectFilePath = path;
        this.logger = logger;
    }


    public bool LoadComparedTypes() {
        if (BindableObject != null && XmlnsDefinitionAttribute != null)
            return true;

        var projectRootDirectory = Path.GetDirectoryName(this.projectFilePath)!;
        var frameworksDirectory = Path.Combine(projectRootDirectory, "bin", "Debug");

        if (!Directory.Exists(frameworksDirectory)) {
            this.logger?.Invoke($"Could not find {frameworksDirectory}. Maybe you need to build your project first?");
            return false;
        }

        // Get newer directory by creation time
        AssembliesDirectory = FindActualAssembliesDirectory(frameworksDirectory);
        if (AssembliesDirectory == null) {
            this.logger?.Invoke("Could not find assemblies directory");
            return false;
        }

        try {
            var controlsAssembly = Assembly.LoadFrom(Path.Combine(AssembliesDirectory, MAUI_CONTROLS_ASSEMBLY));
            BindableObject = controlsAssembly.GetType(MAUI_BINDABLE_OBJECT);
            XmlnsDefinitionAttribute = controlsAssembly.GetType(MAUI_XMLNS_DEFINITION);
            return BindableObject != null && XmlnsDefinitionAttribute != null;
        } catch (Exception e) {
            this.logger?.Invoke($"Could not load {MAUI_CONTROLS_ASSEMBLY}: {e.Message}");
            return false;
        }
    }

    private string? FindActualAssembliesDirectory(string basePath) {
        var assembliesDirectories = new List<string>();
        FindAssembliesDirectory(basePath, assembliesDirectories);

        return assembliesDirectories.OrderByDescending(Directory.GetLastWriteTime).FirstOrDefault();
    }

    private void FindAssembliesDirectory(string basePath, List<string> assembliesDirectories) {
        var assemblies = Directory.GetFiles(basePath, "*.dll", SearchOption.TopDirectoryOnly);

        if (assemblies.Any() && assemblies.Any(x => x.Contains(MAUI_CONTROLS_ASSEMBLY, StringComparison.OrdinalIgnoreCase)))
            assembliesDirectories.Add(basePath);

        foreach (var directory in Directory.GetDirectories(basePath))
            FindAssembliesDirectory(directory, assembliesDirectories);
    }
}
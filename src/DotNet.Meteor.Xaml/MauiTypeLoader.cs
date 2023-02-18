using System.IO.Compression;
using System.Reflection;

namespace DotNet.Meteor.Xaml;

public class MauiTypeLoader {
    public static Type? VisualElement { get; private set; }
    public static Type? XmlnsDefinitionAttribute { get; private set; }
    public string? AssembliesDirectory { get; private set; }

    private readonly Action<string>? logger;
    private readonly string projectFilePath;

    private const string MAUI_CONTROLS_ASSEMBLY = "Microsoft.Maui.Controls.dll";
    private const string MAUI_VISUAL_ELEMENT = "Microsoft.Maui.Controls.VisualElement";
    private const string MAUI_XMLNS_DEFINITION = "Microsoft.Maui.Controls.XmlnsDefinitionAttribute";


    public MauiTypeLoader(string path, Action<string>? logger = null) {
        this.projectFilePath = path;
        this.logger = logger;
    }


    public bool LoadComparedTypes() {
        if (VisualElement != null && XmlnsDefinitionAttribute != null)
            return true;

        var projectRootDirectory = Path.GetDirectoryName(this.projectFilePath)!;
        var frameworksDirectory = Path.Combine(projectRootDirectory, "bin", "Debug");

        if (!Directory.Exists(frameworksDirectory)) {
            this.logger?.Invoke($"Could not find {frameworksDirectory}. Maybe you need to build your project first?");
            return false;
        }

        // Get newer directory by creation time
        AssembliesDirectory = FindAssembliesDirectory(frameworksDirectory);
        if (AssembliesDirectory == null) {
            this.logger?.Invoke("Could not find assemblies directory");
            return false;
        }

        if (AssembliesDirectory.Contains("-android", StringComparison.OrdinalIgnoreCase))
            ExtractMissingAndroidAssemblies(AssembliesDirectory);

        try {
            var controlsAssembly = Assembly.LoadFrom(Path.Combine(AssembliesDirectory, MAUI_CONTROLS_ASSEMBLY));
            VisualElement = controlsAssembly.GetType(MAUI_VISUAL_ELEMENT);
            XmlnsDefinitionAttribute = controlsAssembly.GetType(MAUI_XMLNS_DEFINITION);
            return VisualElement != null && XmlnsDefinitionAttribute != null;
        } catch (Exception e) {
            this.logger?.Invoke($"Could not load {MAUI_CONTROLS_ASSEMBLY}: {e.Message}");
            return false;
        }
    }

    public string? FindAssembliesDirectory(string basePath) {
        var assemblies = Directory.GetFiles(basePath, "*.dll", SearchOption.TopDirectoryOnly);

        if (assemblies.Any())
            return basePath;

        var directories = Directory.GetDirectories(basePath)
            .OrderByDescending(Directory.GetLastWriteTime);
        foreach (var directory in directories) {
            var result = FindAssembliesDirectory(directory);
            if (result != null)
                return result;
        }

        return null;
    }

    private void ExtractMissingAndroidAssemblies(string assembliesDirectory) {
        var apkFile = Directory.GetFiles(assembliesDirectory, "*-Signed.apk", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (apkFile == null) {
            this.logger?.Invoke($"Could not find *-Signed.apk file in {assembliesDirectory}");
            return;
        }

        var tempDirectory = Path.Combine(assembliesDirectory, "_temp");
        Directory.CreateDirectory(tempDirectory);
        ZipFile.ExtractToDirectory(apkFile, tempDirectory);

        var apkAssembliesDirectory = Path.Combine(tempDirectory, "assemblies");
        foreach (var file in Directory.GetFiles(apkAssembliesDirectory, "*.dll", SearchOption.TopDirectoryOnly)) {
            var fileName = Path.GetFileName(file);
            var targetFile = Path.Combine(assembliesDirectory, fileName);
            File.Copy(file, targetFile, true);
        }

        Directory.Delete(tempDirectory, true);
    }
}
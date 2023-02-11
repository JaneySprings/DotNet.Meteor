using System.IO.Compression;
using System.Reflection;

namespace DotNet.Meteor.Xaml;

public class MauiTypeLoader {
    public static Type? VisualElement { get; private set; }
    public static Type? BindableProperty{ get; private set; }
    public string? AssembliesDirecotry { get; private set; }

    private readonly Action<string>? logger;
    private readonly string projectFilePath;
    private readonly string targetFramework;
    private readonly string? runtimeIdentifier;

    private const string MAUI_CONTROLS_ASSEMBLY = "Microsoft.Maui.Controls.dll";
    private const string MAUI_VISUAL_ELEMENT = "Microsoft.Maui.Controls.VisualElement";
    private const string MAUI_BINDABLE_PROPERTY = "Microsoft.Maui.Controls.BindableProperty";


    public MauiTypeLoader(string path, string framework, string? runtimeId=null, Action<string>? logger = null) {
        this.projectFilePath = path;
        this.targetFramework = framework;
        this.runtimeIdentifier = runtimeId;
        this.logger = logger;
    }


    public bool LoadComparedTypes() {
        if (VisualElement != null && BindableProperty != null)
            return true;

        AssembliesDirecotry = FindLibraries();
        if (AssembliesDirecotry == null) {
            this.logger?.Invoke("Could not find assemblies directory");
            return false;
        }

        try {
            var controlsAssembly = Assembly.LoadFrom(Path.Combine(AssembliesDirecotry, MAUI_CONTROLS_ASSEMBLY));
            VisualElement = controlsAssembly.GetType(MAUI_VISUAL_ELEMENT);
            BindableProperty = controlsAssembly.GetType(MAUI_BINDABLE_PROPERTY);
            return VisualElement != null && BindableProperty != null;
        } catch (Exception e) {
            this.logger?.Invoke($"Could not load {MAUI_CONTROLS_ASSEMBLY}: {e.Message}");
            return false;
        }
    }

    private string? FindLibraries() {
        var projectRootDirectory = Path.GetDirectoryName(this.projectFilePath)!;
        var assembliesDirectory = Path.Combine(projectRootDirectory, "bin", "Debug", this.targetFramework);
        if (!string.IsNullOrEmpty(this.runtimeIdentifier))
            assembliesDirectory = Path.Combine(assembliesDirectory, this.runtimeIdentifier);

        if (!Directory.Exists(assembliesDirectory)) {
            this.logger?.Invoke($"Could not find {assembliesDirectory}. Maybe you need to build your project first?");
            return null;
        }

        if (!targetFramework.Contains("android"))
            return assembliesDirectory;

        // Android store all assemblies in the apk file
        var apkFile = Directory.GetFiles(assembliesDirectory, "*-Signed.apk", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (apkFile == null) {
            this.logger?.Invoke($"Could not find *-Signed.apk file in {assembliesDirectory}");
            return null;
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

        return assembliesDirectory;
    }
}
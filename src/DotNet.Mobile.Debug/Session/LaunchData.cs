using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotNet.Mobile.Shared;

namespace DotNet.Mobile.Debug.Session;

public class LaunchData {
    public string AppId { get; set; }
    public string AppName { get; set; }
    public string BundlePath { get; set; }
    public string Target { get; set; }
    public string Framework { get; set; }
    public DeviceData Device { get; set; }
    public Project Project { get; set; }
    public Platform Platform { get; set; }

    public LaunchData(Project project, DeviceData device, string target) {
        Project = project;
        Device = device;
        Target = target;

        AppId = ExtractValueFromProject(Project.Path, "ApplicationId");
        AppName = ExtractValueFromProject(Project.Path, "ApplicationTitle");
        Framework = FindTargetFramework();

        if (device.Platform.Contains("android", StringComparison.OrdinalIgnoreCase)) {
            BundlePath = FindAndroidPackage(Path.GetDirectoryName(Project.Path));
            Platform = Platform.Android;
        } else if (device.Platform.Contains("ios", StringComparison.OrdinalIgnoreCase)) {
            BundlePath = FindApplePackage(Path.GetDirectoryName(Project.Path));
            Platform = Platform.iOS;
        } else {
            Platform = Platform.Undefined;
        }
    }

    private string ExtractValueFromProject(string path, string value) {
        var content = File.ReadAllText(path);
        var regex = new Regex($@"<{value}>(.*?)<\/{value}>", RegexOptions.Singleline);
        var match = regex.Match(content);

        if (!match.Success)
            return Path.GetFileNameWithoutExtension(path);

        return match.Groups[1].Value;
    }

    private string FindTargetFramework() {
        var frameworks = this.Project.Frameworks;
        return frameworks.First(it => it.Contains(this.Device.Platform, StringComparison.OrdinalIgnoreCase));
    }

    private string FindAndroidPackage(string rootDirectory) {
        var binDirectory = Path.Combine(rootDirectory, "bin");
        var files = Directory
            .GetFiles(binDirectory, "*-Signed.apk", SearchOption.AllDirectories)
            .Where(it => it.Contains(Target, StringComparison.OrdinalIgnoreCase));

        if (!files.Any())
            throw new Exception("Could not find Android package");

        return files.First();
    }

    private string FindApplePackage(string rootDirectory) {
        var binDirectory = Path.Combine(rootDirectory, "bin");
        var directories = Directory
            .GetDirectories(binDirectory, "*.app", SearchOption.AllDirectories)
            .Where(it => it.Contains(Target, StringComparison.OrdinalIgnoreCase));

        if (!directories.Any())
            throw new Exception("Could not find ios bundle");

        var armApp = directories.FirstOrDefault(it => it.Contains("arm64", StringComparison.OrdinalIgnoreCase));
        var otherApp = directories.FirstOrDefault(it => !it.Contains("arm64", StringComparison.OrdinalIgnoreCase));

        if (Device.IsEmulator) {
            if (otherApp == null)
                throw new Exception("Could not find bundle for iossimulator");
            return otherApp;
        }

        if (armApp == null)
            throw new Exception("Could not find bundle for ios-arm");
        return armApp;
    }
}

public enum Platform {
    Android,
    iOS,
    Undefined
}
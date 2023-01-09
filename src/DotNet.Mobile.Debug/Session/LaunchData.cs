using System;
using System.IO;
using System.Xml;
using System.Linq;
using DotNet.Mobile.Shared;
using System.Collections.Generic;

namespace DotNet.Mobile.Debug.Session;

public class LaunchData {
    public string AppId { get; }
    public string AppName { get; }
    public string Framework { get; }
    public string ExecutablePath { get; }
    public DeviceData Device { get; }
    public Project Project { get; }
    public bool IsDebug { get; }

    public LaunchData(Project project, DeviceData device, string target) {
        Project = project;
        Device = device;

        IsDebug = target.Equals("debug", StringComparison.OrdinalIgnoreCase);
        Framework = Project.Frameworks.First(it => it.Contains(Device.Platform, StringComparison.OrdinalIgnoreCase));
        Project.Load(new Dictionary<string, string> {
            { "Configuration", target },
            { "TargetFramework", Framework },
            { "RuntimeIdentifier", Device.RuntimeId }
        });

        AppName = Project.EvaluateProperty("ApplicationTitle", "AssemblyName");
        AppId = FindApplicationId();

        if (string.IsNullOrEmpty(AppId))
            AppId = Project.EvaluateProperty("ApplicationId", null, $"{AppName}.{AppName}");

        ExecutablePath = LocateExecutable();
    }

    // TODO: Remove ._Normalize() after MSBuild Api bug fix
    private string LocateExecutable() {
        var rootDirectory = Path.GetDirectoryName(Project.Path);
        var outputDirectory = Path.Combine(rootDirectory, Project.EvaluateProperty("OutputPath"))._Normalize();

        if (!Directory.Exists(outputDirectory))
            throw new DirectoryNotFoundException($"Could not find output directory {outputDirectory}");

        if (Device.IsAndroid) {
            var files = Directory.GetFiles(outputDirectory, $"{AppId}-Signed.apk", SearchOption.TopDirectoryOnly);
            if (!files.Any())
                throw new FileNotFoundException($"Could not find android package in {outputDirectory}");
            return files.FirstOrDefault();
        }

        if (Device.IsWindows) {
            var files = Directory.GetFiles(outputDirectory, $"{AppName}.exe", SearchOption.AllDirectories);
            if (!files.Any())
                throw new FileNotFoundException($"Could not find windows program in {outputDirectory}");
            return files.FirstOrDefault();
        }

        if (Device.IsIPhone || Device.IsMacCatalyst) {
            var bundle = Directory.GetDirectories(outputDirectory, $"{AppName}.app", SearchOption.TopDirectoryOnly);
            if (!bundle.Any())
                throw new DirectoryNotFoundException($"Could not find .app bundle in {outputDirectory}");
            return bundle.FirstOrDefault();
        }

        return null;
    }

    private string FindApplicationId() {
        if (!Device.IsAndroid)
            return null;

        var rootDirectory = Path.GetDirectoryName(Project.Path);
        var manifestPaths = Directory.GetFiles(rootDirectory, "AndroidManifest.xml", SearchOption.AllDirectories);

        if (!manifestPaths.Any())
            return null;

        var xml = new XmlDocument();
        xml.Load(manifestPaths.FirstOrDefault());

        var manifestNode = xml.SelectSingleNode("/manifest");
        var packageAttr = manifestNode.Attributes["package"];

        if (packageAttr == null)
            return null;

        return packageAttr.Value;
    }
}
using System;
using System.IO;
using System.Linq;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Debug.Session;

public class LaunchData {
    public string Framework { get; }
    public string OutputAssembly { get; }
    public DeviceData Device { get; }
    public Project Project { get; }
    public bool IsDebug { get; }

    public LaunchData(Project project, DeviceData device, string target) {
        Project = project;
        Device = device;

        IsDebug = target.Equals("debug", StringComparison.OrdinalIgnoreCase);
        Framework = Project.Frameworks.First(it => it.Contains(Device.Platform, StringComparison.OrdinalIgnoreCase));
        OutputAssembly = Project.GetOutputAssembly(target, Framework, Device);
    }

    public string GetApplicationId() {
        if (Device.IsIPhone || Device.IsMacCatalyst) {
            var workingDirectory = Path.GetDirectoryName(Project.Path);
            var files = Directory.GetFiles(workingDirectory, "Info.plist", SearchOption.AllDirectories)
                .Where(it => !it.Contains(Path.GetFileName(OutputAssembly)));

            if (!files.Any())
                return null;

            var plist = new Apple.PropertyExtractor(files.First());
            return plist.Extract("CFBundleIdentifier") ?? Project.EvaluateProperty("ApplicationId");
        }

        if (!Device.IsAndroid)
            return null;

        var assemblyName = Path.GetFileNameWithoutExtension(OutputAssembly);
        return assemblyName.Replace("-Signed", "");
    }
}
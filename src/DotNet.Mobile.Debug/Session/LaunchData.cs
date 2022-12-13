using System;
using System.IO;
using System.Linq;
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
        Platform = device.IsAndroid ? Platform.Android :
                   device.IsIPhone ? Platform.iOS : Platform.Undefined;

        BundlePath = BundleFinder.FindBundle(Path.GetDirectoryName(project.Path), device, target);
        Framework = Project.Frameworks.First(it =>
            it.Contains(Device.Platform, StringComparison.OrdinalIgnoreCase)
        );

        if (File.Exists(project.Path)) {
            var projectFile = ProjectFile.FromPath(project.Path);
            AppId = projectFile.ValueFromProperty("ApplicationId");
            AppName = projectFile.ValueFromProperty("ApplicationTitle");
            projectFile.Free();
        }
    }
}

public enum Platform {
    Android,
    iOS,
    Undefined
}
using System;
using System.IO;
using System.Text.RegularExpressions;
using DotNet.Mobile.Shared;

namespace DotNet.Mobile.Debug.Session;

public class LaunchData {
    public string AppId { get; set; }
    public string AppName { get; set; }
    public DeviceData Device { get; set; }
    public Project Project { get; set; }
    public Platform Platform { get; set; }

    public LaunchData(Project project, DeviceData device) {
        Project = project;
        Device = device;

        AppId = ExtractValueFromProject(Project.Path, "ApplicationId");
        AppName = ExtractValueFromProject(Project.Path, "ApplicationTitle");

        if (device.Platform.Contains("android", StringComparison.OrdinalIgnoreCase)) {
            Platform = Platform.Android;
        } else if (device.Platform.Contains("ios", StringComparison.OrdinalIgnoreCase)) {
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
           throw new Exception($"Could not find {value} in project file");

        return match.Groups[1].Value;
    }
}

public enum Platform {
    Android,
    iOS,
    Undefined
}
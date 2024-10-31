using System.Text.Json.Serialization;
using DotNet.Meteor.Common.Extensions;
using SystemPath = System.IO.Path;

namespace DotNet.Meteor.Common;

public class Project {
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("path")] public string Path { get; set; }
    [JsonPropertyName("frameworks")] public IEnumerable<string> Frameworks { get; set; }
    [JsonPropertyName("configurations")] public IEnumerable<string> Configurations { get; set; }

    [JsonIgnore] public string Directory => SystemPath.GetDirectoryName(Path)!;

    public Project(string path) {
        Frameworks = Enumerable.Empty<string>();
        Configurations = Enumerable.Empty<string>();
        Name = SystemPath.GetFileNameWithoutExtension(path);
        Path = SystemPath.GetFullPath(path);
    }

    public string GetRelativePath(string? path) {
        path = path?.ToPlatformPath().TrimPathEnd();
        if (string.IsNullOrEmpty(path) || SystemPath.IsPathRooted(path))
            return path ?? string.Empty;

        return SystemPath.Combine(Directory, path);
    }
}
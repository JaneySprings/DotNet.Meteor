using System.Text.Json.Serialization;
using SystemPath = System.IO.Path;

namespace DotNet.Meteor.Common;

public class Project {
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("path")] public string Path { get; set; }
    [JsonPropertyName("frameworks")] public List<string> Frameworks { get; set; }

    [JsonIgnore] public string Directory => SystemPath.GetDirectoryName(Path)!;

    public Project(string path) {
        Frameworks = new List<string>();
        Name = SystemPath.GetFileNameWithoutExtension(path);
        Path = SystemPath.GetFullPath(path);
    }

    public string GetRelativePath(string? path) {
        if (string.IsNullOrEmpty(path) || SystemPath.IsPathRooted(path))
            return path ?? string.Empty;

        return SystemPath.Combine(Directory, path);
    }
}
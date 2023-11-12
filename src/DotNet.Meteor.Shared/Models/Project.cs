using System.Collections.Generic;
using System.Text.Json.Serialization;
using SystemPath = System.IO.Path;

namespace DotNet.Meteor.Shared {
    public class Project {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("path")] public string Path { get; set; }
        [JsonPropertyName("frameworks")] public List<string> Frameworks { get; set; }

        public Project(string path) {
            Frameworks = new List<string>();
            Name = SystemPath.GetFileNameWithoutExtension(path);
            Path = SystemPath.GetFullPath(path);
        }
    }
}
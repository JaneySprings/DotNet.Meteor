using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using SystemPath = System.IO.Path;

namespace DotNet.Meteor.Shared {
    public class Project {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("path")] public string Path { get; set; }
        [JsonProperty("frameworks")] public List<string> Frameworks { get; set; }

        public Project(string path) {
            Frameworks = new List<string>();
            Name = SystemPath.GetFileNameWithoutExtension(path);
            Path = SystemPath.GetFullPath(path);
        }
    }
}
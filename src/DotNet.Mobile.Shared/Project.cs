using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using MSProject = Microsoft.Build.Evaluation.Project;
using MSPath = System.IO.Path;

namespace DotNet.Mobile.Shared {
    public class Project {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("path")] public string Path { get; set; }
        [JsonPropertyName("frameworks")] public List<string> Frameworks { get; set; }

        [JsonIgnore] private MSProject MSProject;

        public Project(string path) {
            Name = MSPath.GetFileNameWithoutExtension(path);
            Path = path;
        }

        public void Load(Dictionary<string, string> properties = null) {
            if (properties != null) {
                var pairs = properties.Where(it => it.Value == null);
                foreach (var pair in pairs)
                    properties.Remove(pair.Key);
            }

            var msbuild = PathUtils.MSBuildAssembly();
            Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", msbuild.FullName);
            MSProject = new MSProject(Path, properties, null);
        }
        public string EvaluateProperty(string name, string fallbackName = null, string defaultValue = null) {
            var value = MSProject.GetPropertyValue(name);

            if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(fallbackName))
                value = MSProject.GetPropertyValue(fallbackName);

            return value == null ? defaultValue : value;
        }
    }
}
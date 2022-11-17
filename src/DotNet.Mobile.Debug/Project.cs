using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace DotNet.Mobile.Debug;

public class Project {
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("path")] public string Path { get; set; }
    [JsonPropertyName("frameworks")] public List<string> Frameworks { get; set; }
}
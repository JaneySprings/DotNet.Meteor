using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol;

public class ExceptionOption {
    [JsonPropertyName("breakMode")] public string BreakMode { get; set; }
    [JsonPropertyName("path")] public List<ExceptionPath> Path { get; set; }
}

public class ExceptionPath {
    [JsonPropertyName("names")] public List<string> Names { get; set; }
}
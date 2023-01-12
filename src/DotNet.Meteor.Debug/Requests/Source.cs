using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol;

public class Source {
    [JsonPropertyName("path")] public string Path { get; set; }
}

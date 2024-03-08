using System.Text.Json.Serialization;

namespace DotNet.Meteor.HotReload.Plugin.Models;

internal class TransferObject {
    [JsonPropertyName("version")] public string? Version { get; set; }
    [JsonPropertyName("xclass")] public string? Definition { get; set; }
    [JsonPropertyName("content")] public string? Content { get; set; }
    [JsonPropertyName("transforms")] public Dictionary<string, string>? Transformations { get; set; }
}

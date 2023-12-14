using System.Text.Json.Serialization;
using CurrentAssembly = System.Reflection.Assembly;

namespace DotNet.Meteor.Xaml;

public class SchemaInfo {
    [JsonPropertyName("version")] public string Version { get; set; }
    [JsonPropertyName("xmlns")] public string? Xmlns { get; set; }
    [JsonPropertyName("assembly")] public string? Assembly { get; set; }
    [JsonPropertyName("types")] public List<TypeInfo>? Types { get; set; }
    [JsonPropertyName("timestamp")] public string? TimeStamp { get; set; }
    [JsonPropertyName("target")] public string? Target { get; set; }

    public SchemaInfo(string? assembly, List<TypeInfo> types) {
        var version = CurrentAssembly.GetExecutingAssembly().GetName().Version;
        Version = $"{version?.Major}.{version?.Minor}.{version?.Build}";
        Assembly = assembly;
        Types = types;
    }
}

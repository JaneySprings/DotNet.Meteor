using System.Text.Json.Serialization;

namespace DotNet.Meteor.Xaml;

public class EnumInfo {
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }

    [JsonPropertyName("doc")] 
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Documentation { get; set; }

    [JsonPropertyName("isObsolete")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsObsolete { get; set; }

    public EnumInfo(string name, string? type) {
        Name = name;
        Type = type;
    }
}
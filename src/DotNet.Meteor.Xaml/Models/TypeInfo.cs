using System.Text.Json.Serialization;

namespace DotNet.Meteor.Xaml;

public class TypeInfo {
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("doc")] public string? Documentation { get; set; }
    [JsonPropertyName("attributes")] public List<AttributeInfo>? Attributes { get; set; }

    public TypeInfo(string name, string? type, List<AttributeInfo>? attributes) {
        Name = name;
        Type = type;
        Attributes = attributes;
    }
}

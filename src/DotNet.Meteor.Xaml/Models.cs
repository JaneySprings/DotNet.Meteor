using System.Text.Json.Serialization;

namespace DotNet.Meteor.Xaml;

public class SchemaInfo {
    [JsonPropertyName("xmlns")] public string? Xmlns { get; set; }
    [JsonPropertyName("types")] public List<TypeInfo>? Types { get; set; }
}

public class TypeInfo {
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("namespace")] public string? Namespace { get; set; }
    [JsonPropertyName("attributes")] public List<AttributeInfo>? Attributes { get; set; }
}

public class AttributeInfo {
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("namespace")] public string? Namespace { get; set; }
    [JsonPropertyName("type")] public object? Type { get; set; }
}
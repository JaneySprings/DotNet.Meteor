using System.Text.Json.Serialization;

namespace DotNet.Meteor.Xaml;

public class SchemaInfo {
    [JsonPropertyName("xmlns")] public string? Xmlns { get; set; }
    [JsonPropertyName("types")] public List<TypeInfo>? Types { get; set; }

    public SchemaInfo(string xmlns, List<TypeInfo> types) {
        Xmlns = xmlns;
        Types = types;
    }
}

public class TypeInfo {
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("namespace")] public string? Namespace { get; set; }
    [JsonPropertyName("doc")] public string? Documentation { get; set; }
    [JsonPropertyName("attributes")] public List<AttributeInfo>? Attributes { get; set; }

    public TypeInfo(string name, string? nspace, List<AttributeInfo>? attributes) {
        Name = name;
        Namespace = nspace;
        Attributes = attributes;
    }
}

public class AttributeInfo {
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("namespace")] public string? Namespace { get; set; }
    [JsonPropertyName("doc")] public string? Documentation { get; set; }
    [JsonPropertyName("type")] public object? Type { get; set; }

    public AttributeInfo(string name, string? nspace, object? type) {
        Name = name;
        Namespace = nspace;
        Type = type;
    }
}
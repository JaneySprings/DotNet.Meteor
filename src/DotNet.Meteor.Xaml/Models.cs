using System.Text.Json.Serialization;

namespace DotNet.Meteor.Xaml;

public class SchemaInfo {
    [JsonPropertyName("xmlns")] public string? Xmlns { get; set; }
    [JsonPropertyName("assembly")] public string? Assembly { get; set; }
    [JsonPropertyName("types")] public List<TypeInfo>? Types { get; set; }
    [JsonPropertyName("timestamp")] public string? TimeStamp { get; set; }
    [JsonPropertyName("target")] public string? Target { get; set; }

    public SchemaInfo(string? assembly, List<TypeInfo> types) {
        Assembly = assembly;
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
    [JsonPropertyName("type")] public object? Type { get; set; }

    [JsonPropertyName("doc")] 
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Documentation { get; set; }

    [JsonPropertyName("isEvent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
     public bool IsEvent { get; set; }

    [JsonPropertyName("isAttached")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsAttached { get; set; }

    public AttributeInfo(string name, string? nspace, object? type, bool isEvent = false, bool isAttached = false) {
        Name = name;
        Namespace = nspace;
        Type = type;
        IsEvent = isEvent;
        IsAttached = isAttached;
    }
}
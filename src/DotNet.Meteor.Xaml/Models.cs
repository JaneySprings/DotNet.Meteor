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

public class AttributeInfo {
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }

    [JsonPropertyName("doc")] 
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Documentation { get; set; }

    [JsonPropertyName("values")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<EnumInfo>? Values { get; set; }

    [JsonPropertyName("isEvent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
     public bool IsEvent { get; set; }

    [JsonPropertyName("isAttached")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsAttached { get; set; }

    [JsonPropertyName("isObsolete")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsObsolete { get; set; }


    public AttributeInfo(string name, string? type) {
        Name = name;
        Type = type;
    }
}

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
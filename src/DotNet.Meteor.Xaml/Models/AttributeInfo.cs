using System.Text.Json.Serialization;

namespace DotNet.Meteor.Xaml;

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

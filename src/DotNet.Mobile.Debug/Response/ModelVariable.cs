using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Protocol;

public class ModelVariable {
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("value")] public string Value { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; }
    [JsonPropertyName("variablesReference")] public int VariablesReference { get; set; }

    public ModelVariable(string name, string value, string type, int variablesReference = 0) {
        this.Name = name;
        this.Value = value;
        this.Type = type;
        this.VariablesReference = variablesReference;
    }
}
using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Protocol;

public class ModelScope {
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("variablesReference")] public int VariablesReference { get; set; }
    [JsonPropertyName("expensive")] public bool Expensive { get; set; }

    public ModelScope(string name, int variablesReference, bool expensive = false) {
        this.Name = name;
        this.VariablesReference = variablesReference;
        this.Expensive = expensive;
    }
}
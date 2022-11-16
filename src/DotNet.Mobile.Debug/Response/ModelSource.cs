using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Protocol;

public class ModelSource {
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("path")] public string Path { get; set; }
    [JsonPropertyName("sourceReference")] public int SourceReference { get; set; }
    [JsonPropertyName("presentationHint")] public string PresentationHint { get; set; }

    public ModelSource(string name, string path, int sourceReference, string hint) {
        this.Name = name;
        this.Path = path;
        this.SourceReference = sourceReference;
        this.PresentationHint = hint;
    }
}
using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Protocol;

public class ModelStackFrame {
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("source")] public ModelSource Source { get; set; }
    [JsonPropertyName("line")] public int Line { get; set; }
    [JsonPropertyName("column")] public int Column { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("presentationHint")] public string PresentationHint { get; set; }

    public ModelStackFrame(int id, string name, ModelSource source, int line, int column, string hint) {
        this.Id = id;
        this.Name = name;
        this.Source = source;

        // These should NEVER be negative
        this.Line = System.Math.Max(0, line);
        this.Column = System.Math.Max(0, column);

        this.PresentationHint = hint;
    }
}
using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol;

public class EvaluateResponseBody {
    [JsonPropertyName("result")] public string Result { get; set; }
    [JsonPropertyName("variablesReference")] public int VariablesReference { get; set; }

    public EvaluateResponseBody(string value, int reference = 0) {
        this.Result = value;
        this.VariablesReference = reference;
    }
}
using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol;

public class BodyError {
    [JsonPropertyName("error")] public ModelMessage Error { get; set; }

    public BodyError(ModelMessage error) {
        this.Error = error;
    }
}
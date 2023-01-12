using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol;

public class ModelBreakpoint {
    [JsonPropertyName("verified")] public bool Verified { get; set; }
    [JsonPropertyName("line")] public int Line { get; set; }

    public ModelBreakpoint(bool verified, int line) {
        this.Verified = verified;
        this.Line = line;
    }
}
using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol;

public class ProtocolBase {
    [JsonPropertyName("seq")] public int Seq { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; }

    public ProtocolBase(string type = null, int seq = 0) {
        this.Type = type;
        this.Seq = seq;
    }
}
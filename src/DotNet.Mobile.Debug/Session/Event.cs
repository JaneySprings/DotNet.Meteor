using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Session;

public class Event : ProtocolMessage {
    [JsonPropertyName("event")] public string EventType { get; }
    [JsonPropertyName("body")] public dynamic Body { get; }

    public Event(string type, dynamic bdy = null) : base("event") {
        EventType = type;
        Body = bdy;
    }
}
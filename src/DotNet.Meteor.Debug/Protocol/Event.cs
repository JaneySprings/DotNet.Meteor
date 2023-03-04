using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol;

/* A debug adapter initiated event. */
public class Event : ProtocolMessage {
    /* Type of event. */
    [JsonPropertyName("event")] public string EventType { get; set; }

    /* Event-specific information. */
    [JsonPropertyName("body")] public object Body_ { get; set; }

    public Event(string type, object body = null) : base("event") {
        this.EventType = type;
        this.Body_ = body;
    }
}
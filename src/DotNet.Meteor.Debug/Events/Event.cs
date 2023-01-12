using System.Text.Json.Serialization;
using DotNet.Meteor.Debug.Protocol;

namespace DotNet.Meteor.Debug.Events;

public class Event : ProtocolBase {
    public const string ExitedEvent = "exited";
    public const string InitializedEvent = "initialized";
    public const string OutputEvent = "output";
    public const string StoppedEvent = "stopped";
    public const string TerminatedEvent = "terminated";
    public const string ThreadEvent = "thread";

    [JsonPropertyName("event")] public string EventType { get; set; }
    [JsonPropertyName("body")] public object Body { get; set; }

    public Event(string type, object body = null) : base("event") {
        this.EventType = type;
        this.Body = body;
    }
}
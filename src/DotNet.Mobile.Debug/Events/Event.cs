using Newtonsoft.Json;
using DotNet.Mobile.Debug.Protocol;

namespace DotNet.Mobile.Debug.Events;

public class Event : ProtocolMessage {
    [JsonProperty(PropertyName = "event")]
    public string eventType { get; }
    public dynamic body { get; }

    public Event(string type, dynamic bdy = null) : base("event") {
        eventType = type;
        body = bdy;
    }
}
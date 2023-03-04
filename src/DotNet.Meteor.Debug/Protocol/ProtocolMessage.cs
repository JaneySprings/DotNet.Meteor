using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol;

/* Base class of requests, responses, and events. */
public abstract class ProtocolMessage {
    /* Sequence number of the message (also known as message ID). The `seq` for
    * the first message sent by a client or debug adapter is 1, and for each
    * subsequent message is 1 greater than the previous message sent by that
    * actor. `seq` can be used to order requests, responses, and events, and to
    * associate requests with their corresponding responses. For protocol
    * messages of type `request` the sequence number can be used to cancel the
    * request.
    */
    [JsonPropertyName("seq")] public int Seq { get; set; }

    /* Message type.
    * Values: 'request', 'response', 'event', etc. */
    [JsonPropertyName("type")] public string Type { get; set; }

    protected ProtocolMessage(string type = null, int seq = 1) {
        this.Type = type;
        this.Seq = seq;
    }
}
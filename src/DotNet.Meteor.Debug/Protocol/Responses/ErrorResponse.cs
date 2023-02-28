using System.Text.Json.Serialization;
using DotNet.Meteor.Debug.Protocol.Types;

namespace DotNet.Meteor.Debug.Protocol;

/* On error (whenever success is false), the body can provide more details. */
public class ErrorResponse: Response {
    public ErrorResponse(Request request, Body body): base(request, false, body) {}

    public class Body {
        /* A structured error message. */
        [JsonPropertyName("error")] public Message Error { get; set; }
    }
}
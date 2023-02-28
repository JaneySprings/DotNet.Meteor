using System;
using System.Text.Json.Serialization;
using DotNet.Meteor.Debug.Protocol.Types;

namespace DotNet.Meteor.Debug.Protocol;

/* On error (whenever success is false), the body can provide more details. */
public class ErrorResponseBody {
    /* A structured error message. */
    [JsonPropertyName("error")] public Message Error { get; set; }

    public ErrorResponseBody() {}
    public ErrorResponseBody(Exception exception) {
        this.Error = new Message($"{exception.Message}\n{exception.StackTrace}", exception.HResult);
    }
}
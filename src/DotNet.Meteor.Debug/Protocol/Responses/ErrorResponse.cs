using System;
using System.Text.Json.Serialization;
using DotNet.Meteor.Debug.Protocol.Types;
using DotNet.Meteor.Logging;

namespace DotNet.Meteor.Debug.Protocol;

/* On error (whenever success is false), the body can provide more details. */
public class ErrorResponseBody {
    /* A structured error message. */
    [JsonPropertyName("error")] public Message Error { get; set; }

    public ErrorResponseBody() {}
    public ErrorResponseBody(Exception exception) {
        this.Error = new Message(exception.Message, exception.HResult);
        this.Error.Url = $"file://{LogConfig.ErrorLogFile}";
        this.Error.UrlLabel = "View Log";
    }
}
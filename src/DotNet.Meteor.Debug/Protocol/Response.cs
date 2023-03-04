using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol;

/* Response for a request. */
public class Response : ProtocolMessage {
    /* Outcome of the request.
    * If true, the request was successful and the `body` attribute may contain
    * the result of the request.
    * If the value is false, the attribute `message` contains the error in short
    * form and the `body` may contain additional information (see
    * `ErrorResponse.body.error`). */
    [JsonPropertyName("success")] public bool Success { get; set; }

    /* Contains the raw error in short form if `success` is false.
    * This raw error might be interpreted by the client and is not shown in the
    * UI.
    * Some predefined values exist.
    * Values: 
    * 'cancelled': the request was cancelled.
    * 'notStopped': the request may be retried once the adapter is in a 'stopped'
    * state. */
    [JsonPropertyName("message")] public string Message { get; set; }

    /* Sequence number of the corresponding request. */
    [JsonPropertyName("request_seq")] public int RequestSeq { get; set; }

    /* The command requested. */
    [JsonPropertyName("command")] public string Command { get; set; }

    /* Contains request result if success is true and error details if success is
    * false. */
    [JsonPropertyName("body")] public object Body { get; set; }

    public Response() {}
    public Response(Request request) : base("response") {
        this.RequestSeq = request.Seq;
        this.Command = request.Command;
    }

    public void SetSuccess(object body = null) {
        this.Body = body;
        this.Success = true;
    }
    public void SetError(string message = null, object body = null) {
        this.Body = body;
        this.Success = false;
        this.Message = message;
    }
}
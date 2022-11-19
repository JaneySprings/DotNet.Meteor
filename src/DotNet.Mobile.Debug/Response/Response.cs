using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Protocol;

public class Response : ProtocolBase {
    [JsonPropertyName("success")] public bool Success { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; }
    [JsonPropertyName("request_seq")] public int RequestSeq { get; set; }
    [JsonPropertyName("command")] public string Command { get; set; }
    [JsonPropertyName("body")] public object Body { get; set; }

    public Response() {}
    public Response(Request request) : base("response") {
        this.Success = true;
        this.RequestSeq = request.Seq;
        this.Command = request.Command;
    }

    public void SetBody(object body) {
        this.Success = true;
        this.Body = body;
    }

    public void SetBodyError(string msg, object body = null) {
        this.Success = false;
        this.Message = msg;
        this.Body = body;
    }
}
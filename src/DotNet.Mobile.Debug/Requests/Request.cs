using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Protocol;

public class Request : ProtocolBase {
    [JsonPropertyName("command")] public string Command { get; set; }
    [JsonPropertyName("arguments")] public Argument Arguments { get; set; }

    public Request(string cmd, Argument arg, int id = 0) : base("request", id) {
        this.Command = cmd;
        this.Arguments = arg;
    }
}
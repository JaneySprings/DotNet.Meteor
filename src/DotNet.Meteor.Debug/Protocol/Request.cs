using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol;

/* A client or debug adapter initiated request. */
public class Request : ProtocolMessage {
    /* The command to execute. */
    [JsonPropertyName("command")] public string Command { get; set; }

    /* Object containing arguments for the command. */
    [JsonPropertyName("arguments")] public Arguments Arguments { get; set; }

    public Request() {}
}
using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Session;

public class Request : ProtocolMessage {
    [JsonPropertyName("command")] public string Command { get; set; }
    [JsonPropertyName("arguments")] public Argument Arguments { get; set; }

    public Request() {}

    public Request(string cmd, Argument arg) : base("request") {
        this.Command = cmd;
        this.Arguments = arg;
    }
    public Request(int id, string cmd, Argument arg) : base("request", id) {
        this.Command = cmd;
        this.Arguments = arg;
    }

    public class Argument {
        [JsonPropertyName("clientID")] public string ClientID { get; set; }
        [JsonPropertyName("clientName")] public string ClientName { get; set; }
        [JsonPropertyName("adapterID")] public string AdapterID { get; set; }
        [JsonPropertyName("pathFormat")] public string PathFormat { get; set; }
        [JsonPropertyName("linesStartAt1")] public bool LinesStartAt1 { get; set; }
        [JsonPropertyName("columnsStartAt1")] public bool ColumnsStartAt1 { get; set; }
        [JsonPropertyName("supportsVariableType")] public bool SupportsVariableType { get; set; }
        [JsonPropertyName("supportsVariablePaging")] public bool SupportsVariablePaging { get; set; }
        [JsonPropertyName("supportsRunInTerminalRequest")] public bool SupportsRunInTerminalRequest { get; set; }
        [JsonPropertyName("locale")] public string Locale { get; set; }
        [JsonPropertyName("supportsProgressReporting")] public bool SupportsProgressReporting { get; set; }
        [JsonPropertyName("supportsInvalidatedEvent")] public bool SupportsInvalidatedEvent { get; set; }
        [JsonPropertyName("supportsMemoryReferences")] public bool SupportsMemoryReferences { get; set; }
        [JsonPropertyName("supportsArgsCanBeInterpretedByShell")] public bool SupportsArgsCanBeInterpretedByShell { get; set; }
    }
}
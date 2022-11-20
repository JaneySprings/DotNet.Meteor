using System.Text.Json.Serialization;
using System.Collections.Generic;
using DotNet.Mobile.Shared;

namespace DotNet.Mobile.Debug.Protocol;

public class Argument {
    [JsonPropertyName("linesStartAt1")] public bool LinesStartAt1 { get; set; }
    [JsonPropertyName("pathFormat")] public string PathFormat { get; set; }
    [JsonPropertyName("expression")] public string Expression { get; set; }
    [JsonPropertyName("address")] public string Address { get; set; }
    [JsonPropertyName("threadId")] public int ThreadId { get; set; }
    [JsonPropertyName("frameId")] public int FrameId { get; set; }
    [JsonPropertyName("variablesReference")] public int VariablesReference { get; set; } = -1;
    [JsonPropertyName("levels")] public int Levels { get; set; } = 10;
    [JsonPropertyName("lines")] public List<int> Lines { get; set; }
    [JsonPropertyName("__exceptionOptions")] public List<ExceptionOption> ExceptionOptions { get; set; }
    [JsonPropertyName("source")] public Source Source { get; set; }

    // Custom arguments
    [JsonPropertyName("debugging_port")] public int DebuggingPort { get; set; }
    [JsonPropertyName("selected_device")] public DeviceData Device { get; set; }
    [JsonPropertyName("selected_project")] public Project Project { get; set; }
}
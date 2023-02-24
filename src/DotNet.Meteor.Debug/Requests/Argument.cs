using System.Text.Json.Serialization;
using System.Collections.Generic;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Debug.Protocol;

public class Argument {
    [JsonPropertyName("linesStartAt1")] public bool LinesStartAt1 { get; set; }
    [JsonPropertyName("pathFormat")] public string PathFormat { get; set; }
    [JsonPropertyName("expression")] public string Expression { get; set; }
    [JsonPropertyName("variablesReference")] public int VariablesReference { get; set; } = -1;
    [JsonPropertyName("threadId")] public int ThreadId { get; set; }
    [JsonPropertyName("frameId")] public int FrameId { get; set; }
    [JsonPropertyName("levels")] public int Levels { get; set; }
    [JsonPropertyName("startFrame")] public int StartFrame { get; set; }
    [JsonPropertyName("lines")] public List<int> Lines { get; set; }
    [JsonPropertyName("__exceptionOptions")] public List<ExceptionOption> ExceptionOptions { get; set; }
    [JsonPropertyName("source")] public Source Source { get; set; }

    // LaunchConfig arguments
    [JsonPropertyName("debugging_port")] public int DebuggingPort { get; set; }
    [JsonPropertyName("selected_device")] public DeviceData Device { get; set; }
    [JsonPropertyName("selected_project")] public Project Project { get; set; }
    [JsonPropertyName("selected_target")] public string Target { get; set; }
}
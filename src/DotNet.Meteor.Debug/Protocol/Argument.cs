using System.Text.Json.Serialization;
using System.Collections.Generic;
using DotNet.Meteor.Shared;
using DotNet.Meteor.Debug.Protocol.Types;

namespace DotNet.Meteor.Debug.Protocol;

public class Arguments {
    [JsonPropertyName("expression")] public string Expression { get; set; }
    [JsonPropertyName("variablesReference")] public int VariablesReference { get; set; } = -1;
    [JsonPropertyName("threadId")] public int ThreadId { get; set; }
    [JsonPropertyName("frameId")] public int FrameId { get; set; }
    [JsonPropertyName("levels")] public int Levels { get; set; }
    [JsonPropertyName("startFrame")] public int StartFrame { get; set; }
    [JsonPropertyName("source")] public Source Source { get; set; }
    [JsonPropertyName("breakpoints")] public List<Breakpoint> Breakpoints { get; set; }

    // LaunchConfig arguments
    [JsonPropertyName("debugging_port")] public int DebuggingPort { get; set; }
    [JsonPropertyName("selected_device")] public DeviceData Device { get; set; }
    [JsonPropertyName("selected_project")] public Project Project { get; set; }
    [JsonPropertyName("selected_target")] public string Target { get; set; }
}
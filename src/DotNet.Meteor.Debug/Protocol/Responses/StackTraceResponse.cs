using System.Collections.Generic;
using System.Text.Json.Serialization;
using DotNet.Meteor.Debug.Protocol.Types;

namespace DotNet.Meteor.Debug.Protocol;

public class StackTraceResponseBody {
    [JsonPropertyName("stackFrames")] public List<StackFrame> StackFrames { get; set; }
    [JsonPropertyName("totalFrames")] public int TotalFrames { get; set; }

    public StackTraceResponseBody(List<StackFrame> frames, int total) {
        this.StackFrames = frames;
        this.TotalFrames = total;
    }
}
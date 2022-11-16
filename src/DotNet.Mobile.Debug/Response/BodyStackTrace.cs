using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Protocol;

public class BodyStackTrace {
    [JsonPropertyName("stackFrames")] public List<ModelStackFrame> StackFrames { get; set; }
    [JsonPropertyName("totalFrames")] public int TotalFrames { get; set; }

    public BodyStackTrace(List<ModelStackFrame> frames, int total) {
        this.StackFrames = frames;
        this.TotalFrames = total;
    }
}
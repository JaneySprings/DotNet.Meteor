using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Protocol;

public class BodySetBreakpoints {
    [JsonPropertyName("breakpoints")] public List<ModelBreakpoint> Breakpoints { get; set; }

    public BodySetBreakpoints(List<ModelBreakpoint> breakpoints = null) {
        this.Breakpoints = breakpoints ?? new List<ModelBreakpoint>();
    }
}
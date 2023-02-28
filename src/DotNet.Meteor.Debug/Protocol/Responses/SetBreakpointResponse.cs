using System.Collections.Generic;
using System.Text.Json.Serialization;
using DotNet.Meteor.Debug.Protocol.Types;

namespace DotNet.Meteor.Debug.Protocol;

public class SetBreakpointsResponseBody {
    [JsonPropertyName("breakpoints")] public List<Breakpoint> Breakpoints { get; set; }

    public SetBreakpointsResponseBody(List<Breakpoint> breakpoints = null) {
        this.Breakpoints = breakpoints ?? new List<Breakpoint>();
    }
}
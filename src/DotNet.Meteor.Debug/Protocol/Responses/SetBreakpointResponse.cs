using System.Collections.Generic;
using System.Text.Json.Serialization;
using DotNet.Meteor.Debug.Protocol.Types;

namespace DotNet.Meteor.Debug.Protocol;

public class SetBreakpointsResponseBody {
    /* Information about the breakpoints.
    * The array elements are in the same order as the elements of the
    * `breakpoints` (or the deprecated `lines`) array in the arguments. */
    [JsonPropertyName("breakpoints")] public List<Breakpoint> Breakpoints { get; set; }

    public SetBreakpointsResponseBody(List<Breakpoint> breakpoints = null) {
        this.Breakpoints = breakpoints ?? new List<Breakpoint>();
    }
}
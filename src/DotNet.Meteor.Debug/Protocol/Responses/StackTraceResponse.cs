using System.Collections.Generic;
using System.Text.Json.Serialization;
using DotNet.Meteor.Debug.Protocol.Types;

namespace DotNet.Meteor.Debug.Protocol;

public class StackTraceResponseBody {
    /* The frames of the stack frame. If the array has length zero, there are no
    * stack frames available.
    * This means that there is no location information available. */
    [JsonPropertyName("stackFrames")] public List<StackFrame> StackFrames { get; set; }

    /* The total number of frames available in the stack. If omitted or if
    * `totalFrames` is larger than the available frames, a client is expected
    * to request frames until a request returns less frames than requested
    * (which indicates the end of the stack). Returning monotonically
    * increasing `totalFrames` values for subsequent requests can be used to
    * enforce paging in the client. */
    [JsonPropertyName("totalFrames")] public int TotalFrames { get; set; }

    public StackTraceResponseBody(List<StackFrame> frames, int total) {
        this.StackFrames = frames;
        this.TotalFrames = total;
    }
}
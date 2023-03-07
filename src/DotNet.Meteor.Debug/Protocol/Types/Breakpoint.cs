using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol.Types;

/* Information about a breakpoint created in setBreakpoints, setFunctionBreakpoints,
 * setInstructionBreakpoints, or setDataBreakpoints requests. */
public class Breakpoint {
    /* If true, the breakpoint could be set (but not necessarily at the desired
    * location). */
    [JsonPropertyName("verified")] public bool Verified { get; set; }

    /* The start line of the actual range covered by the breakpoint. */
    [JsonPropertyName("line")] public int Line { get; set; }

    /* An optional start column of the actual range covered by the breakpoint. */
    [JsonPropertyName("column")] public int? Column { get; set; }

    /* An optional expression for conditional breakpoints. */
    [JsonPropertyName("condition")] public string Condition { get; set; }

    public Breakpoint() { }
    public Breakpoint(bool verified, int line, int? column = null) {
        this.Verified = verified;
        this.Line = line;
        this.Column = column;
    }
}
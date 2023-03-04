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

    /* An optional end line of the actual range covered by the breakpoint. */
    [JsonPropertyName("endLine")] public int? EndLine { get; set; }

    /* An optional end column of the actual range covered by the breakpoint. If no
    * end line is given, then the end column is assumed to be in the start line. */
    [JsonPropertyName("endColumn")] public int? EndColumn { get; set; }

    public Breakpoint(bool verified, int line, int? column = null, int? endLine = null, int? endColumn = null) {
        this.Verified = verified;
        this.Line = line;
        this.Column = column;
        this.EndLine = endLine;
        this.EndColumn = endColumn;
    }
}
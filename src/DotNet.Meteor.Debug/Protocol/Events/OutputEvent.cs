using System.Text.Json.Serialization;
using DotNet.Meteor.Debug.Protocol.Types;

namespace DotNet.Meteor.Debug.Protocol.Events;

/* The event indicates that the target has produced some output. */
public class OutputEvent: Event {
    public OutputEvent(OutputEvent.Body body) : base("output", body) { }

    public class Body {
        /* The output category. If not specified or if the category is not
        * understood by the client, `console` is assumed.
        * Values: 
        * 'console': Show the output in the client's default message UI, e.g. a
        * 'debug console'. This category should only be used for informational
        * output from the debugger (as opposed to the debuggee).
        * 'important': A hint for the client to show the output in the client's UI
        * for important and highly visible information, e.g. as a popup
        * notification. This category should only be used for important messages
        * from the debugger (as opposed to the debuggee). Since this category value
        * is a hint, clients might ignore the hint and assume the `console`
        * category.
        * 'stdout': Show the output as normal program output from the debuggee.
        * 'stderr': Show the output as error program output from the debuggee.
        * 'telemetry': Send the output to telemetry instead of showing it to the
        * user. */
        [JsonPropertyName("category")] public string Category { get; set; }

        /* The output to report. */
        [JsonPropertyName("output")] public string Output { get; set; }

        /* If an attribute `variablesReference` exists and its value is > 0, the
        * output contains objects which can be retrieved by passing
        * `variablesReference` to the `variables` request as long as execution
        * remains suspended. See 'Lifetime of Object References' in the Overview
        * section for details. */
        [JsonPropertyName("variablesReference")] public int VariablesReference { get; set; } = -1;

        /* An optional source location where the output was produced. */
        [JsonPropertyName("source")] public Source Source { get; set; }

        /* An optional source location line where the output was produced. */
        [JsonPropertyName("line")] public int Line { get; set; }

        /* An optional source location column where the output was produced. */
        [JsonPropertyName("column")] public int Column { get; set; }

        public Body(string category, string output) {
            this.Category = category;
            this.Output = output;
        }
    }
}
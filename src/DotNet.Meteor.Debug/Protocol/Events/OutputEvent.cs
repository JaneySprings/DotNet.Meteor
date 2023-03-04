using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol.Events;

/* The event indicates that the target has produced some output. */
public class OutputEvent: Event {
    public OutputEvent(string category, string output) : base("output") {
        this.Body_ = new Body {
            Category = category,
            Output = output
        };
    }

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
    }
}
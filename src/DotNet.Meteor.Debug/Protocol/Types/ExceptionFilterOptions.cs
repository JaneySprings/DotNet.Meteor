using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol.Types;

/* An ExceptionFilterOptions is used to specify an exception filter together with a
* condition for the setExceptionBreakpoints request. */
public class ExceptionFilterOptions {
    /* ID of an exception filter returned by the `exceptionBreakpointFilters`
    * capability. */
    [JsonPropertyName("filterId")] public string FilterId { get; set; }

    /* An expression for conditional exceptions.
    * The exception breaks into the debugger if the result of the condition is
    * true. */
    [JsonPropertyName("condition")] public string Condition { get; set; }
}
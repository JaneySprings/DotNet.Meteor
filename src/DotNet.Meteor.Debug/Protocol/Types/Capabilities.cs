using System.Collections.Generic;
using System.Text.Json.Serialization;
using DotNet.Meteor.Debug.Protocol.Types;

namespace DotNet.Meteor.Debug.Protocol;

/* Information about the capabilities of a debug adapter. */
public class Capabilities {
    /* The debug adapter supports function breakpoints. */
    [JsonPropertyName("supportsFunctionBreakpoints")] public bool SupportsFunctionBreakpoints { get; set; }

    /* The debug adapter supports conditional breakpoints. */
    [JsonPropertyName("supportsConditionalBreakpoints")] public bool SupportsConditionalBreakpoints { get; set; }

    /* The debug adapter supports breakpoints that break execution after a specified number of hits. */
    [JsonPropertyName("supportsHitConditionalBreakpoints")] public bool SupportsHitConditionalBreakpoints { get; set; }

    /* The debug adapter supports a (side effect free) evaluate request for data hovers. */
    [JsonPropertyName("supportsEvaluateForHovers")] public bool SupportsEvaluateForHovers { get; set; }

    /* Available filters or options for the setExceptionBreakpoints request. */
    [JsonPropertyName("exceptionBreakpointFilters")] public List<ExceptionBreakpointsFilter> ExceptionBreakpointFilters { get; set; }

    /* The debug adapter supports stepping back via the 'stepBack' and 'reverseContinue' requests. */
    [JsonPropertyName("supportsStepBack")] public bool SupportsStepBack { get; set; }

    /* The debug adapter supports 'exceptionOptions' on the setExceptionBreakpoints request. */
    [JsonPropertyName("supportsExceptionOptions")] public bool SupportsExceptionOptions { get; set; }

    /* The debug adapter supports `filterOptions` as an argument on the
    * setExceptionBreakpoints` request. */
    [JsonPropertyName("supportsExceptionFilterOptions")] public bool SupportsExceptionFilterOptions { get; set; }

    /* The debug adapter supports the 'exceptionInfo' request. */
    [JsonPropertyName("supportsExceptionInfoRequest")] public bool SupportsExceptionInfoRequest { get; set; }

    /* The debug adapter supports logpoints by interpreting the 'logMessage' attribute of the SourceBreakpoint. */
    [JsonPropertyName("supportsLogPoints")] public bool SupportsLogPoints { get; set; }
}
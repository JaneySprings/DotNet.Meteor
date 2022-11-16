using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Protocol;

public class BodyCapabilities {
    [JsonPropertyName("supportsConfigurationDoneRequest")] public bool SupportsConfigurationDoneRequest { get; set; }
    [JsonPropertyName("supportsFunctionBreakpoints")] public bool SupportsFunctionBreakpoints { get; set; }
    [JsonPropertyName("supportsConditionalBreakpoints")] public bool SupportsConditionalBreakpoints { get; set; }
    [JsonPropertyName("supportsEvaluateForHovers")] public bool SupportsEvaluateForHovers { get; set; }
    [JsonPropertyName("exceptionBreakpointFilters")] public List<object> ExceptionBreakpointFilters { get; set; }
}
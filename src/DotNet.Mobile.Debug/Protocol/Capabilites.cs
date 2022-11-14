namespace DotNet.Mobile.Debug.Protocol;

public class Capabilities : ResponseBody {
    public bool supportsConfigurationDoneRequest;
    public bool supportsFunctionBreakpoints;
    public bool supportsConditionalBreakpoints;
    public bool supportsEvaluateForHovers;
    public dynamic[] exceptionBreakpointFilters;
}
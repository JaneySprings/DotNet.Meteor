using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol.Types;

/* An ExceptionBreakpointsFilter is shown in the UI as an filter option 
* for configuring how exceptions are dealt with. */
public class ExceptionBreakpointsFilter {

    /* The internal ID of the filter. This value is passed to the setExceptionBreakpoints request. */
    [JsonPropertyName("filter")] public string Filter { get; set; }

    /* The name of the filter. This will be shown in the UI. */
    [JsonPropertyName("label")] public string Label { get; set; }

    /* A help text providing additional information about the exception filter.
    * This string is typically shown as a hover and can be translated. */
    [JsonPropertyName("description")] public string Description { get; set; }

    /* Initial value of the filter. If not specified a value 'false' is assumed. */
    [JsonPropertyName("default")] public bool Default { get; set; }

    /* Controls whether a condition can be specified for this filter option. If
    * false or missing, a condition can not be set. */
    [JsonPropertyName("supportsCondition")] public bool SupportsCondition { get; set; }

    /* A help text providing information about the condition. This string is shown
    * as the placeholder text for a text box and can be translated. */
    [JsonPropertyName("conditionDescription")] public string ConditionDescription { get; set; }

    public ExceptionBreakpointsFilter() { }

    public static ExceptionBreakpointsFilter AllExceptions => new ExceptionBreakpointsFilter {
        Filter = "all",
        Label = "All Exceptions",
        Description = "Break when an exception is thrown.",
        Default = true,
        SupportsCondition = true,
        ConditionDescription = "Specifies a exception name to break on. Example: System.Exception."
    };
}
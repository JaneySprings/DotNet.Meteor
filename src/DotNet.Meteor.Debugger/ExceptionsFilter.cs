using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;

namespace DotNet.Meteor.Debugger.Extensions;

public static class ExceptionsFilter {
    public static ExceptionBreakpointsFilter AllExceptions => new ExceptionBreakpointsFilter {
        Filter = "all",
        Label = "All Exceptions",
        Description = "Break when an exception is thrown.",
        ConditionDescription = "Comma-separated list of exception types to break on, or if the list starts with '!', a list of exception types to ignore.",
        SupportsCondition = true
    };
}
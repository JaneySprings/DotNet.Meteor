using System.Diagnostics;

namespace DotNet.Meteor.HotReload.Plugin;

internal static class Logger {
    internal static void Log(string message) {
        Debug.WriteLine($"[HotReload]: {message}");
    }
    internal static void LogError(string message) {
        Debug.WriteLine($"[HotReload][Error]: {message}");
    }
    internal static void LogError(Exception exception) {
        var currentException = exception;
        do {
            LogError(currentException.Message);
            currentException = currentException.InnerException;
        } while (currentException?.InnerException != null);
    }
}
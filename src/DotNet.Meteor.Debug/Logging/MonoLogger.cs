using System;
using Mono.Debugging.Client;
using NLog;

namespace DotNet.Meteor.Debug;

public class MonoLogger : ICustomLogger {
    private readonly Logger logger = LogManager.GetCurrentClassLogger();

    public void Log(string message) => logger.Debug($"Mono: {message}");
    public void LogError(string message, Exception ex) => logger.Error(ex, message);
    public void LogAndShowException(string message, Exception ex) => logger.Error(ex, message);
    public void LogMessage(string format, params object[] args) => logger.Debug($"Mono: {format}", args);
    public string GetNewDebuggerLogFilename() => null;
}

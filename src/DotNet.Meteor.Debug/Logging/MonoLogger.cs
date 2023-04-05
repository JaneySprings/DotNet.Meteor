using System;
using Mono.Debugging.Client;
using NLog;

namespace DotNet.Meteor.Debug;

public class MonoLogger : ICustomLogger {
    private readonly Logger logger = LogManager.GetCurrentClassLogger();

    public string GetNewDebuggerLogFilename() => nameof(MonoLogger);
    public void LogMessage(string format, params object[] args) => logger.Debug(format, args);
    public void LogAndShowException(string message, Exception ex) => LogError(message, ex);
    public void LogError(string message, Exception ex) {
        if (ex == null) {
            logger.Error(message);
            return;
        }
        logger.Error(ex, message);
        var innerException = ex.InnerException;

        while (innerException != null) {
            logger.Error(innerException);
            innerException = innerException.InnerException;
        }
    }
}

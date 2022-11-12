using System;
using Mono.Debugging.Client;
using DotNet.Mobile.Shared;

namespace DotNet.Mobile.Debug.Session;

public class MonoLogger : ICustomLogger {
    public void LogError(string message, Exception ex) {
        Logger.Info(message);
    }

    public void LogAndShowException(string message, Exception ex) {
        Logger.Info(message + "\n" + ex.StackTrace);
    }

    public void LogMessage(string format, params object[] args) {
        Logger.Info(string.Format(format, args));
    }

    public string GetNewDebuggerLogFilename() {
        return null;
    }
}
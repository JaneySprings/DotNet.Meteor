using System;
using Mono.Debugging.Client;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Debug.Session;

public class MonoLogger : ICustomLogger {
    public void LogError(string message, Exception ex) => Logger.Log(ex);
    public void LogAndShowException(string message, Exception ex) => Logger.Log(ex);
    public void LogMessage(string format, params object[] args) => Logger.Log(format, args);
    public string GetNewDebuggerLogFilename() => null;
}

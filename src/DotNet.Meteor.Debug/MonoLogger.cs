using System;
using System.IO;
using Mono.Debugging.Client;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Debug;

public class MonoLogger : ICustomLogger {
    public void Log(string message) {}
    public void LogError(string message, Exception ex) {}
    public void LogAndShowException(string message, Exception ex) {}
    public void LogMessage(string format, params object[] args) {}
    public string GetNewDebuggerLogFilename() => null;
}

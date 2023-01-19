using System;
using System.IO;
using Mono.Debugging.Client;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Debug;

public class MonoLogger : ICustomLogger {
    private static Logger logger;
    private static Logger InnerLogger => logger ??= new Logger("debug_session");

    private static MonoLogger instance;
    public static MonoLogger Instance => instance ??= new MonoLogger();

    public void Log(string message) => InnerLogger.Log(message);
    public void LogError(string message, Exception ex) => InnerLogger.Log(ex);
    public void LogAndShowException(string message, Exception ex) => InnerLogger.Log(ex);
    public void LogMessage(string format, params object[] args) => InnerLogger.Log(format, args);
    public string GetNewDebuggerLogFilename() => Path.GetFileName(InnerLogger.LogFile);
}

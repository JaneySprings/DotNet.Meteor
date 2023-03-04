using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace DotNet.Meteor.Logging;

public static class LogConfig {
    private static readonly string _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
    public static readonly string ErrorLogFile = Path.Combine(_logDir, "Error.log");
    public static readonly string DebugLogFile = Path.Combine(_logDir, "Debug.log");

    public static void InitializeLog() {
        var conf = new LoggingConfiguration();

        var commonTarget = new FileTarget() {
            FileName = DebugLogFile,
            Layout = "${time}|${message}",
            DeleteOldFileOnStartup = true,
        };
        var commonAsyncTarget = new AsyncTargetWrapper(commonTarget, 500, AsyncTargetWrapperOverflowAction.Discard);
        conf.AddTarget("log", commonAsyncTarget);

        var errorTarget = new FileTarget() {
            MaxArchiveFiles = 1,
            ArchiveNumbering = ArchiveNumberingMode.Date,
            ArchiveOldFileOnStartup = true,
            FileName = ErrorLogFile,
            Layout = "${longdate}|${message}|${callsite}|${stacktrace:format=Raw}",
            DeleteOldFileOnStartup = true
        };
        var errorAsyncTarget = new AsyncTargetWrapper(errorTarget, 500, AsyncTargetWrapperOverflowAction.Discard);
        conf.AddTarget("errorLog", errorAsyncTarget);

        conf.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, commonAsyncTarget));
        conf.LoggingRules.Add(new LoggingRule("*", LogLevel.Error, errorAsyncTarget));

        LogManager.ThrowExceptions = false;
        LogManager.Configuration = conf;
        LogManager.ReconfigExistingLoggers();
    }
}
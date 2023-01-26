using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace DotNet.Meteor.Logging;

public static class LogConfig {
    public static void InitializeLog() {
        var conf = new LoggingConfiguration();
        var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        var commonTarget = new FileTarget() {
            FileName = Path.Combine(logsDir, "Debug.log"),
            Layout = "${time}|${message}",
            DeleteOldFileOnStartup = true,
        };
        var commonAsyncTarget = new AsyncTargetWrapper(commonTarget, 500, AsyncTargetWrapperOverflowAction.Discard);
        conf.AddTarget("log", commonAsyncTarget);

        var errorTarget = new FileTarget() {
            MaxArchiveFiles = 1,
            ArchiveNumbering = ArchiveNumberingMode.Date,
            ArchiveOldFileOnStartup = true,
            FileName = Path.Combine(logsDir, "Error.log"),
            Layout = "${longdate}|${message}|${stacktrace}",
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
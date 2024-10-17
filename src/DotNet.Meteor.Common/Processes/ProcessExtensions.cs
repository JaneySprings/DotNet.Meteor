using System.Diagnostics;
using DotNet.Meteor.Common.Processes;

namespace DotNet.Meteor.Common;

public static class ProcessExtensions {
    // private const int ExitTimeout = 1000;

    public static void Terminate(this Process process) {
        if (!process.HasExited) {
            process.Kill();
            // process.WaitForExit(ExitTimeout);
        }
        process.Close();
    }
}

public class CatchTargetLogger : IProcessLogger {
    private readonly IProcessLogger innerLogger;
    private readonly Action handler;
    private readonly string catchTarget;

    public CatchTargetLogger(string catchTarget, IProcessLogger innerLogger, Action handler) {
        this.innerLogger = innerLogger;
        this.handler = handler;
        this.catchTarget = catchTarget;
    }

    void IProcessLogger.OnErrorDataReceived(string stderr) {
        innerLogger.OnErrorDataReceived(stderr);
        if (stderr.Contains(catchTarget, StringComparison.OrdinalIgnoreCase))
            handler.Invoke();
    }

    void IProcessLogger.OnOutputDataReceived(string stdout) {
        innerLogger.OnOutputDataReceived(stdout);
        if (stdout.Contains(catchTarget, StringComparison.OrdinalIgnoreCase))
            handler.Invoke();
    }
}
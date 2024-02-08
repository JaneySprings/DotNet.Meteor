using System;
using System.Linq;
using System.Threading.Tasks;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;

namespace DotNet.Meteor.Debug.Extensions;

public static class MonoExtensions {
    public const int TerminationTimeout = 3000;

    public static string ToThreadName(this string threadName, int threadId) {
        if (!string.IsNullOrEmpty(threadName))
            return threadName;
        if (threadId == 1)
            return "Main Thread";
        return $"Thread #{threadId}";
    }
    public static string ToDisplayValue(this ObjectValue value) {
        var dv = value.DisplayValue ?? "<error getting value>";
        if (dv.Length > 1 && dv[0] == '{' && dv[dv.Length - 1] == '}')
            dv = dv.Substring(1, dv.Length - 2).Replace(Environment.NewLine, " ");
        return dv;
    }
    public static StackFrame GetFrameSafe(this Backtrace bt, int n) {
		try {
            return bt.GetFrame(n);
        } catch (Exception) {
            return null;
        }
	}
    public static ThreadInfo FindThread(this SoftDebuggerSession session, long id) {
        var process = session.GetProcesses().FirstOrDefault();
        if (process == null)
            return null;

        return process.GetThreads().FirstOrDefault(it => it.Id == id);
    }
    public static ExceptionInfo FindException(this SoftDebuggerSession session, long id) {
        var thread = session.FindThread(id);
        if (thread == null)
            return null;

        for (int i = 0; i < thread.Backtrace.FrameCount; i++) {
            var frame = thread.Backtrace.GetFrameSafe(i);
            var ex = frame?.GetException();
            if (ex != null)
                return ex;
        }

        return null;
    }
    public static bool Exit(this SoftDebuggerSession session, int millisecondsTimeout) {
        if (session == null)
            return true;

        return Task.Run(() => session.Exit()).Wait(millisecondsTimeout);
    }
}
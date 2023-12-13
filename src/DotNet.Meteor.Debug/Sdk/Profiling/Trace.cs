using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Meteor.Debug.Extensions;
using DotNet.Meteor.Processes;
using TraceCollectHandler = Microsoft.Diagnostics.Tools.Trace.CollectCommandHandler;
// using GCDumpCollectHandler = Microsoft.Diagnostics.Tools.GCDump.CollectCommandHandler;

namespace DotNet.Meteor.Debug.Sdk.Profiling;

public static class Trace {

    public static ProfilingTask Collect(int pid, string outputFile, IProcessLogger logger) {
        return CollectCore(pid, string.Empty, outputFile, logger);
    }
    public static ProfilingTask Collect(string diagnosticPort, string outputFile, IProcessLogger logger) {
        return CollectCore(0, diagnosticPort, outputFile, logger);
    }

    private static ProfilingTask CollectCore(int pid, string diagnosticPort, string outputFile, IProcessLogger logger) {
        var cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;

        TraceCollectHandler.ProcessLogger.WriteLine = logger.OnOutputDataReceived;
        TraceCollectHandler.ProcessLogger.ErrorWriteLine = logger.OnErrorDataReceived;

        var task = Task.Run(async() => await TraceCollectHandler.Collect(token, pid, new FileInfo(outputFile), diagnosticPort));
        return new ProfilingTask(task, cancellationTokenSource);
    }
}
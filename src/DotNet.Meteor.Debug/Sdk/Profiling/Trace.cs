using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Meteor.Debug.Extensions;
using DotNet.Meteor.Processes;
using TraceFileFormat = Microsoft.Diagnostics.Tools.Trace.TraceFileFormat;
using TraceCollectHandler = Microsoft.Diagnostics.Tools.Trace.CollectCommandHandler;

namespace DotNet.Meteor.Debug.Sdk.Profiling;

public static class Trace {

    public static ProfilerTask Collect(int pid, string outputFile, IProcessLogger logger) {
        return CollectCore(pid, string.Empty, outputFile, logger);
    }
    public static ProfilerTask Collect(string diagnosticPort, string outputFile, IProcessLogger logger) {
        return CollectCore(0, diagnosticPort, outputFile, logger);
    }

    private static ProfilerTask CollectCore(int pid, string diagnosticPort, string outputFile, IProcessLogger logger) {
        var cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;
        var fileFormat = TraceFileFormat.Speedscope;
        var providers = string.Empty;

        if (TraceCollectHandler.ProcessLogger.WriteLine == null)
            TraceCollectHandler.ProcessLogger.WriteLine = logger.OnOutputDataReceived;
        if (TraceCollectHandler.ProcessLogger.ErrorWriteLine == null)
            TraceCollectHandler.ProcessLogger.ErrorWriteLine = logger.OnErrorDataReceived;

        var task = Task.Run(async() => await TraceCollectHandler.Collect(token, pid, new FileInfo(outputFile), fileFormat, diagnosticPort, providers));
        return new ProfilerTask(task, cancellationTokenSource);
    }
}
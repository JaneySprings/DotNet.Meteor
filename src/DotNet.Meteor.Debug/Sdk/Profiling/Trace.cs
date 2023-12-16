using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Meteor.Debug.Extensions;
using DotNet.Meteor.Processes;
using TraceFileFormat = Microsoft.Diagnostics.Tools.Trace.TraceFileFormat;
using TraceCollectHandler = Microsoft.Diagnostics.Tools.Trace.CollectCommandHandler;
// using GCDumpCollectHandler = Microsoft.Diagnostics.Tools.GCDump.CollectCommandHandler;

namespace DotNet.Meteor.Debug.Sdk.Profiling;

public static class Trace {

    public static ProfilingTask Collect(int pid, string outputFile, string mode, IProcessLogger logger) {
        return CollectCore(pid, string.Empty, outputFile, mode, logger);
    }
    public static ProfilingTask Collect(string diagnosticPort, string outputFile, string mode, IProcessLogger logger) {
        return CollectCore(0, diagnosticPort, outputFile, mode, logger);
    }

    private static ProfilingTask CollectCore(int pid, string diagnosticPort, string outputFile, string mode, IProcessLogger logger) {
        var cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;
        var fileFormat = TraceFileFormat.Speedscope;
        var providers = string.Empty;

        if (mode?.Equals("gcdump", StringComparison.OrdinalIgnoreCase) == true) {
            //"Microsoft-DotNETRuntimeMonoProfiler:0x8900001:4:";
            //"Microsoft-DotNETRuntimeMonoProfiler:0xC900001:4";
            //"Microsoft-DotNETRuntimeMonoProfiler:0xC900003:4:heapcollect=ondemand";
            //"Microsoft-DotNETRuntimeMonoProfiler:0x4000000:4";
            providers = "Microsoft-DotNETRuntimeMonoProfiler:0xC900001:4";
            fileFormat = TraceFileFormat.NetTrace;
        }

        if (TraceCollectHandler.ProcessLogger.WriteLine == null)
            TraceCollectHandler.ProcessLogger.WriteLine = logger.OnOutputDataReceived;
        if (TraceCollectHandler.ProcessLogger.ErrorWriteLine == null)
            TraceCollectHandler.ProcessLogger.ErrorWriteLine = logger.OnErrorDataReceived;

        var task = Task.Run(async() => await TraceCollectHandler.Collect(token, pid, new FileInfo(outputFile), fileFormat, diagnosticPort, providers));
        return new ProfilingTask(task, cancellationTokenSource);
    }
}
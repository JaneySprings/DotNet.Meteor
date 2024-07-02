using System.IO;
using System.Threading;
using DotNet.Meteor.Debug.Extensions;
using DotNet.Meteor.Processes;
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
        var task = TraceCollectHandler.Collect(
            ct: token,
            console: new ConsoleLogger(logger),
            processId: pid,
            output: new FileInfo(outputFile),
            buffersize: TraceCollectHandler.DefaultCircularBufferSizeInMB(),
            providers: string.Empty,
            profile: string.Empty,
            format: Microsoft.Diagnostics.Tools.Trace.TraceFileFormat.Speedscope,
            duration: default,
            clrevents: string.Empty,
            clreventlevel: string.Empty,
            name: null,
            diagnosticPort: diagnosticPort,
            showchildio: false,
            resumeRuntime: true,
            stoppingEventProviderName: null,
            stoppingEventEventName: null,
            stoppingEventPayloadFilter: null,
            rundown: null
        );

        return new ProfilerTask(task, cancellationTokenSource);
    }
}
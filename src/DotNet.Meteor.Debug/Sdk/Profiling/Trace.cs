using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Meteor.Debug.Extensions;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;
using Microsoft.Diagnostics.Tools.Trace;

namespace DotNet.Meteor.Debug.Sdk.Profiling;

public static class Trace {
    // private static FileInfo TraceTool() {
    //     string homeDirectory = RuntimeSystem.HomeDirectory;
    //     string path = Path.Combine(homeDirectory, ".dotnet", "tools", "dotnet-trace" + RuntimeSystem.ExecExtension);

    //     if (!File.Exists(path))
    //         throw new FileNotFoundException("Could not find trace tool. Please install it with 'dotnet tool install --global dotnet-dsrouter'");

    //     return new FileInfo(path);
    // }

    // [Obsolete($"Use {nameof(CollectCommandHandler)} instead.")]
    // public static Process Collect(int pid, string outputFile, IProcessLogger logger = null) {
    //     var trace = Trace.TraceTool();
    //     var arguments = new ProcessArgumentBuilder()
    //         .Append("collect")
    //         .Append("-p", pid.ToString())
    //         .Append("-o", outputFile)
    //         .Append("--format", "speedscope");
    //     return new ProcessRunner(trace, arguments, logger).Start();
    // }

    // [Obsolete($"Use {nameof(CollectCommandHandler)} instead.")]
    // public static Process Collect(string diagnosticPort, string outputFile, IProcessLogger logger = null) {
    //     var trace = Trace.TraceTool();
    //     var arguments = new ProcessArgumentBuilder()
    //         .Append("collect")
    //         .Append("--diagnostic-port", diagnosticPort)
    //         .Append("-o", outputFile)
    //         .Append("--format", "speedscope");
    //     return new ProcessRunner(trace, arguments, logger).Start();
    // }

    public static ProfilingTask Collect(int pid, string outputFile, IProcessLogger logger) {
        return CollectCore(pid, string.Empty, outputFile, logger);
    }
    public static ProfilingTask Collect(string diagnosticPort, string outputFile, IProcessLogger logger) {
        return CollectCore(0, diagnosticPort, outputFile, logger);
    }

    private static ProfilingTask CollectCore(int pid, string diagnosticPort, string outputFile, IProcessLogger logger) {
        var cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;

        CollectCommandHandler.Logger.WriteLine = logger.OnOutputDataReceived;
        CollectCommandHandler.Logger.ErrorWriteLine = logger.OnErrorDataReceived;

        var task = Task.Run(async() => await CollectCommandHandler.Collect(token, pid, new FileInfo(outputFile), diagnosticPort));
        return new ProfilingTask(task, cancellationTokenSource);
    }
}
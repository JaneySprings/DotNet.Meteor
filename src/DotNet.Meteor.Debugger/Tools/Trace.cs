using DotNet.Meteor.Common;
using DotNet.Meteor.Common.Processes;
using DotNet.Meteor.Debugger.Extensions;

namespace DotNet.Meteor.Debugger.Tools;

public static class Trace {
    public static FileInfo TraceTool() {
        string assembliesDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string path = Path.Combine(assembliesDirectory, "dotnet-trace" + RuntimeSystem.ExecExtension);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Could not find {nameof(Trace)} tool.");

        return new FileInfo(path);
    }

    public static ProfilerTask Collect(int pid, string outputFile, IProcessLogger logger) {
        var process = new ProcessRunner(Trace.TraceTool(), new ProcessArgumentBuilder()
            .Append("collect")
            .Append("--process-id", pid.ToString())
            .Append("--output").AppendQuoted(outputFile)
            .Append("--format", "Speedscope"), logger)
            .Start();
        return new ProfilerTask(process);
    }
    public static ProfilerTask Collect(string diagnosticPort, string outputFile, IProcessLogger logger) {
        var process = new ProcessRunner(Trace.TraceTool(), new ProcessArgumentBuilder()
            .Append("collect")
            .Append("--dport").AppendQuoted(diagnosticPort)
            .Append("--output").AppendQuoted(outputFile)
            .Append("--format", "Speedscope"), logger)
            .Start();
        return new ProfilerTask(process);
    }
}
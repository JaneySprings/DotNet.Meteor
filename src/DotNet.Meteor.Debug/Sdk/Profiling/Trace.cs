using System.Diagnostics;
using System.IO;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Debug.Sdk.Profiling;

public static class Trace {
    public static FileInfo TraceTool() {
        string homeDirectory = RuntimeSystem.HomeDirectory;
        string path = Path.Combine(homeDirectory, ".dotnet", "tools", "dotnet-trace" + RuntimeSystem.ExecExtension);

        if (!File.Exists(path))
            throw new FileNotFoundException("Could not find trace tool. Please install it with 'dotnet tool install --global dotnet-dsrouter'");

        return new FileInfo(path);
    }

    public static Process Collect(int pid, string outputFile, IProcessLogger logger = null) {
        var trace = Trace.TraceTool();
        var arguments = new ProcessArgumentBuilder()
            .Append("collect")
            .Append("-p", pid.ToString())
            .Append("-o", outputFile)
            .Append("--format", "speedscope");
        return new ProcessRunner(trace, arguments, logger).Start();
    }

    public static Process Collect(string diagnosticPort, string outputFile, IProcessLogger logger = null) {
        var trace = Trace.TraceTool();
        var arguments = new ProcessArgumentBuilder()
            .Append("collect")
            .Append("--diagnostic-port", diagnosticPort)
            .Append("-o", outputFile)
            .Append("--format", "speedscope");
        return new ProcessRunner(trace, arguments, logger).Start();
    }
}
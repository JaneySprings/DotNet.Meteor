using System;
using System.Diagnostics;
using System.IO;
using DotNet.Meteor.Common;
using DotNet.Meteor.Processes;

namespace DotNet.Meteor.Debug.Sdk.Profiling;

public static class GCDump {
    public static FileInfo GCDumpTool() {
        string assembliesDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string path = Path.Combine(assembliesDirectory, "dotnet-gcdump" + RuntimeSystem.ExecExtension);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Could not find {nameof(GCDump)} tool.");

        return new FileInfo(path);
    }

    public static Process Collect(int pid, string outputFile, string additionalArgs, IProcessLogger logger) {
        var gcdump = GCDump.GCDumpTool();
        var arguments = new ProcessArgumentBuilder()
            .Append("collect")
            .Append("-p", pid.ToString())
            .Append("-o").AppendQuoted(outputFile);

        if (!string.IsNullOrEmpty(additionalArgs))
            arguments.Append(additionalArgs);

        return new ProcessRunner(gcdump, arguments, logger).Start();
    }
    public static Process Collect(string diagnosticPort, string outputFile, string additionalArgs, IProcessLogger logger) {
        var gcdump = GCDump.GCDumpTool();
        var arguments = new ProcessArgumentBuilder()
            .Append("collect")
            .Append("--dport", diagnosticPort)
            .Append("-o").AppendQuoted(outputFile);

        if (!string.IsNullOrEmpty(additionalArgs))
            arguments.Append(additionalArgs);

        return new ProcessRunner(gcdump, arguments, logger).Start();
    }
}
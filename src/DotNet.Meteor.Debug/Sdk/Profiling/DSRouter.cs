using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Debug.Sdk.Profiling;

public static class DSRouter {
    public static FileInfo DSRouterTool() {
        string assembliesDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string path = Path.Combine(assembliesDirectory, "dotnet-dsrouter" + RuntimeSystem.ExecExtension);

        if (!File.Exists(path))
            throw new FileNotFoundException("Could not find dsrouter tool.");

        return new FileInfo(path);
    }

    public static Process ServerToServer(int port, IProcessLogger logger = null) {
        var dsrouter = DSRouter.DSRouterTool();
        var arguments = new ProcessArgumentBuilder()
            .Append("server-server")
            .Append("-tcps", $"127.0.0.1:{port}");
        return new ProcessRunner(dsrouter, arguments, logger).Start();
    }

    public static Process ClientToServer(int port, IProcessLogger logger = null) {
        var dsrouter = DSRouter.DSRouterTool();
        var arguments = new ProcessArgumentBuilder()
            .Append("client-server")
            .Append("-tcps", $"127.0.0.1:{port}");
        return new ProcessRunner(dsrouter, arguments, logger).Start();
    }

    // public static ProfilingTask ServerToServer(string ipc, string tcp) {
    //     var cancellationTokenSource = new CancellationTokenSource();
    //     var token = cancellationTokenSource.Token;
    //     var commands = new DiagnosticsServerRouterCommands();
    //     var task = Task.Run(async() => await commands.RunIpcServerTcpServerRouter(token, ipc, tcp, 0, "none", string.Empty));
    //     return new ProfilingTask(task, cancellationTokenSource);
    // }

    // public static ProfilingTask ClientToServer(string ipc, string tcp) {
    //     var cancellationTokenSource = new CancellationTokenSource();
    //     var token = cancellationTokenSource.Token;
    //     var commands = new DiagnosticsServerRouterCommands();
    //     var task = Task.Run(async() => await commands.RunIpcClientTcpServerRouter(token, ipc, tcp, 0, "none", string.Empty));
    //     return new ProfilingTask(task, cancellationTokenSource);
    // }
}
using System.Diagnostics;
using System.IO;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Debug.Sdk.Profiling;

public static class DSRouter {
    public static FileInfo DSRouterTool() {
        string homeDirectory = RuntimeSystem.HomeDirectory;
        string path = Path.Combine(homeDirectory, ".dotnet", "tools", "dotnet-dsrouter" + RuntimeSystem.ExecExtension);

        if (!File.Exists(path))
            throw new FileNotFoundException("Could not find dsrouter tool. Please install it with 'dotnet tool install --global dotnet-dsrouter'");

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

    public static Process ServerToClient(int port, IProcessLogger logger = null) {
        var dsrouter = DSRouter.DSRouterTool();
        var arguments = new ProcessArgumentBuilder()
            .Append("server-client")
            .Append("-tcpc", $"127.0.0.1:{port}")
            .Append("--forward-port iOS");
        return new ProcessRunner(dsrouter, arguments, logger).Start();
    }
}
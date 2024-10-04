using System.Diagnostics;
using DotNet.Meteor.Common;
using DotNet.Meteor.Common.Processes;

namespace DotNet.Meteor.Debug.Tools;

public static class DSRouter {
    public static FileInfo DSRouterTool() {
        string assembliesDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string path = Path.Combine(assembliesDirectory, "dotnet-dsrouter" + RuntimeSystem.ExecExtension);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Could not find {nameof(DSRouter)} tool.");

        return new FileInfo(path);
    }

    public static Process ServerToServer(int port, IProcessLogger? logger = null) {
        var dsrouter = DSRouter.DSRouterTool();
        var arguments = new ProcessArgumentBuilder()
            .Append("server-server")
            .Append("-tcps", $"127.0.0.1:{port}");
        return new ProcessRunner(dsrouter, arguments, logger).Start();
    }

    public static Process ClientToServer(string ipcc, string tcps, IProcessLogger? logger = null) {
        var dsrouter = DSRouter.DSRouterTool();
        var arguments = new ProcessArgumentBuilder()
            .Append("client-server")
            .Append("-ipcc", ipcc)
            .Append("-tcps", tcps);
        return new ProcessRunner(dsrouter, arguments, logger).Start();
    }

    public static Process ServerToClient(string ipcs, string tcpc, bool forwardApple = false, IProcessLogger? logger = null) {
        var dsrouter = DSRouter.DSRouterTool();
        var arguments = new ProcessArgumentBuilder()
            .Append("server-client")
            .Append("-ipcs", ipcs)
            .Append("-tcpc", tcpc);

        if (forwardApple)
            arguments.Append("--forward-port", "iOS");

        return new ProcessRunner(dsrouter, arguments, logger).Start();
    }
}
using System.IO.Pipes;
using Mono.Debugging.Client;
using StreamJsonRpc;

namespace DotNet.Meteor.Debug;

public class ExternalTypeResolver : IDisposable {
    private readonly NamedPipeClientStream? pipeStream;
    private JsonRpc? rpcServer;

    public ExternalTypeResolver(string? transportId) {
        if (!string.IsNullOrEmpty(transportId))
            pipeStream = new NamedPipeClientStream(".", transportId, PipeDirection.InOut, PipeOptions.Asynchronous);
    }

    public bool TryConnect(int timeoutMs = 5000) {
        if (pipeStream == null)
            return false;

        try {
            pipeStream.Connect(timeoutMs);
            rpcServer = JsonRpc.Attach(pipeStream);
        } catch (Exception) {
            Dispose();
            return false;
        }

        DebuggerLoggingService.CustomLogger.LogMessage("Debugger connected to external type resolver");
        return true;
    }

    public string? Resolve(string identifierName, SourceLocation location, bool _) {
        if (rpcServer == null)
            return null;

        var task = rpcServer.InvokeAsync<string>("HandleResolveType", identifierName, location);
        return task.Result;
    }

    public void Dispose() {
        rpcServer?.Dispose();
        pipeStream?.Dispose();
    }
}
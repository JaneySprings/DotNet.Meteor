using System;
using System.IO;
using Mono.Debugger.Soft;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;

namespace DotNet.Mobile.Debug.Pipeline;

class StreamCommandConnectionDebuggerArgs : SoftDebuggerStartArgs, ISoftDebuggerConnectionProvider {
    readonly string appName;

    public StreamCommandConnectionDebuggerArgs(string appName, StreamCommandConnection commandConnection) {
        this.appName = appName;
        CommandConnection = commandConnection;
    }

    public StreamCommandConnection CommandConnection { get; }

    public override ISoftDebuggerConnectionProvider ConnectionProvider => this;

    IAsyncResult ISoftDebuggerConnectionProvider.BeginConnect(DebuggerStartInfo dsi, AsyncCallback callback) {
        return CommandConnection.BeginStartDebugger(callback, null);
    }

    void ISoftDebuggerConnectionProvider.EndConnect(IAsyncResult result, out VirtualMachine vm, out string appName) {
        appName = this.appName;

        CommandConnection.EndStartDebugger(result, out Stream transport, out Stream output);
        var transportConnection = new IPhoneTransportConnection(CommandConnection, transport);
        var outputReader = new StreamReader(output);
        vm = VirtualMachineManager.Connect(transportConnection, outputReader, null);
    }

    void ISoftDebuggerConnectionProvider.CancelConnect(IAsyncResult result) {
        CommandConnection.CancelStartDebugger(result);
    }

    bool ISoftDebuggerConnectionProvider.ShouldRetryConnection(Exception ex) {
        return false;
    }
}
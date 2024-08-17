using System;
using Mono.Debugger.Soft;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;
using System.Net;
using System.Net.Sockets;
using DotNet.Meteor.Debug.Extensions;

namespace DotNet.Meteor.Debug.Sdb;

public class ServerConnectionProvider : SoftDebuggerStartArgs, ISoftDebuggerConnectionProvider {
    private readonly TcpListener listener;
    private readonly string appName;

    public ServerConnectionProvider(IPAddress host, int port, string appName) {
        this.appName = appName;
        this.listener = new TcpListener(new IPEndPoint(host, port));
        this.listener.Start();
    }

    public override ISoftDebuggerConnectionProvider ConnectionProvider => this;

    public IAsyncResult BeginConnect(DebuggerStartInfo dsi, AsyncCallback callback) {
        return this.listener.BeginAcceptSocket(callback, null);
    }
    public void EndConnect(IAsyncResult result, out VirtualMachine vm, out string appName) {
        var socket = this.listener.EndAcceptSocket(result);
        var stream = new NetworkStream(socket);

        this.WriteSdbCommand(stream, "start debugger: sdb");

        var transportConnection = new ServerConnection(this.listener, stream);
        vm = VirtualMachineManager.Connect(transportConnection, null, null);
        appName = this.appName;
    }
    public void CancelConnect(IAsyncResult result) {
        this.listener.Stop();
    }
    public bool ShouldRetryConnection(Exception ex) {
        return false;
    }
}
using Mono.Debugger.Soft;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;
using System.Net;
using System.Net.Sockets;
using DotNet.Meteor.Debug.Extensions;

namespace DotNet.Meteor.Debug.Sdb;

public class ClientConnectionProvider : SoftDebuggerStartArgs, ISoftDebuggerConnectionProvider {
    private TcpClient client = null!;
    private readonly string appName;
    private readonly IPEndPoint endPoint;

    public ClientConnectionProvider(IPAddress host, int port, string appName) {
        this.appName = appName;
        this.endPoint = new IPEndPoint(host, port);
        // On Windows and Linux we need to wait for the app to start manually
        // One minute should be enough
        MaxConnectionAttempts = 120;
        TimeBetweenConnectionAttempts = 500;
    }

    public override ISoftDebuggerConnectionProvider ConnectionProvider => this;

    public IAsyncResult BeginConnect(DebuggerStartInfo dsi, AsyncCallback callback) {
        this.client = new TcpClient();
        return this.client.BeginConnect(this.endPoint.Address, this.endPoint.Port, callback, null);
    }
    public void EndConnect(IAsyncResult result, out VirtualMachine vm, out string appName) {
        this.client.EndConnect(result);
        var stream = this.client.GetStream();

        this.WriteSdbCommand(stream, "start debugger: sdb");

        var transportConnection = new ClientConnection(this.client, stream);
        vm = VirtualMachineManager.Connect(transportConnection, null, null);
        appName = this.appName;
    }
    public void CancelConnect(IAsyncResult result) {
        this.client.Close();
    }
    public bool ShouldRetryConnection(Exception ex) {
        return true;
    }
}
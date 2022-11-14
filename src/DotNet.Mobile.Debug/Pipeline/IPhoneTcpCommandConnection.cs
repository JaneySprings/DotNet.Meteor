using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace DotNet.Mobile.Debug.Pipeline;

class IPhoneTcpCommandConnection : StreamCommandConnection {
    private readonly Socket wifiListener;

    public IPhoneTcpCommandConnection(IPAddress ipAddress = null, int port = 0) {
        ipAddress ??= IPAddress.Any;
        Port = port;

        this.wifiListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {
            ExclusiveAddressUse = false
        };
        this.wifiListener.Bind(new IPEndPoint(ipAddress, Port));
        if (Port == 0) {
            Port = ((IPEndPoint)this.wifiListener.LocalEndPoint).Port;
        }
        this.wifiListener.Listen(15);
    }

    protected override IAsyncResult BeginConnectStream(AsyncCallback callback, object state) {
        return this.wifiListener.BeginAccept(callback, state);
    }

    protected override Stream EndConnectStream(IAsyncResult result) {
        Socket socket = this.wifiListener.EndAccept(result);
        socket.NoDelay = true;
        return new NetworkStream(socket, true);
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        this.wifiListener?.Dispose();
    }
}
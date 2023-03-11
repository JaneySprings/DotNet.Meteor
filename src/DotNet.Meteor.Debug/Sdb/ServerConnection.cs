using System.IO;
using System.Net.Sockets;
using Mono.Debugger.Soft;

namespace DotNet.Meteor.Debug.Sdb;

public class ServerConnection : Connection {
    readonly TcpListener listener;
    readonly Stream stream;

    internal ServerConnection(TcpListener listener, Stream stream) {
        this.listener = listener;
        this.stream = stream;
    }

    protected override int TransportSend(byte[] buf, int buf_offset, int len) {
        this.stream.Write(buf, buf_offset, len);
        return len;
    }

    protected override int TransportReceive(byte[] buf, int buf_offset, int len) {
        return this.stream.Read(buf, buf_offset, len);
    }

    protected override void TransportSetTimeouts(int send_timeout, int receive_timeout) {}

    protected override void TransportClose() {
        this.listener.Stop();
        this.stream.Close();
    }
    protected override void TransportShutdown() {
        this.stream.Dispose();
    }
}
using System.IO;
using System.Net.Sockets;
using Mono.Debugger.Soft;

namespace DotNet.Meteor.Debug.Sdb;

public class ClientConnection : Connection {
    readonly TcpClient client;
    readonly Stream stream;

    internal ClientConnection(TcpClient client, Stream stream) {
        this.client = client;
        this.stream = stream;
    }

    protected override int TransportSend(byte[] buf, int buf_offset, int len) {
        this.stream.Write(buf, buf_offset, len);
        return len;
    }

    protected override int TransportReceive(byte[] buf, int buf_offset, int len) {
        return this.stream.Read(buf, buf_offset, len);
    }

    protected override void TransportSetTimeouts(int send_timeout, int receive_timeout) {
        this.client.SendTimeout = send_timeout;
        this.client.ReceiveTimeout = receive_timeout;
    }

    protected override void TransportClose() {
        this.client.Close();
        this.stream.Close();
    }
    protected override void TransportShutdown() {
        this.client.Dispose();
        this.stream.Dispose();
    }
}
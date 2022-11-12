using System.IO;
using DotNet.Mobile.Shared;

namespace DotNet.Mobile.Debug.Session;

class IPhoneTransportConnection : Mono.Debugger.Soft.Connection {
    readonly StreamCommandConnection connection;
    readonly Stream stream;

    internal IPhoneTransportConnection(StreamCommandConnection connection, Stream stream) {
        this.connection = connection;
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
        Logger.Warning($"StreamTransportConnection.TransportSetTimeouts ({send_timeout}, {receive_timeout}): Not supported");
    }

    protected override void TransportClose() {
        this.connection.Dispose();
        this.stream.Close();
    }
    protected override void TransportShutdown() {
        this.connection.Dispose();
        this.stream.Close();
    }

}
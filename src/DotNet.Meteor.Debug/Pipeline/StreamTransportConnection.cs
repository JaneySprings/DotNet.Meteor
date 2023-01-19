using System.IO;
using Mono.Debugger.Soft;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Debug.Pipeline;

class IPhoneTransportConnection : Connection {
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
        MonoLogger.Instance.LogMessage("StreamTransportConnection.TransportSetTimeouts ({0}, {1}): Not supported", send_timeout, receive_timeout);
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
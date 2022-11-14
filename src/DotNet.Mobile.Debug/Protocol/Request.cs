namespace DotNet.Mobile.Debug.Protocol;

public class Request : ProtocolMessage {
    public string command;
    public dynamic arguments;

    public Request() {}
    public Request(string cmd, dynamic arg) : base("request") {
        this.command = cmd;
        this.arguments = arg;
    }
    public Request(int id, string cmd, dynamic arg) : base("request", id) {
        this.command = cmd;
        this.arguments = arg;
    }
}
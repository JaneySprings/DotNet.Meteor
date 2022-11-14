namespace DotNet.Mobile.Debug.Protocol;

public class ResponseBody {}

public class Response : ProtocolMessage {
    public bool success;
    public string message;
    public int request_seq;
    public string command;
    public ResponseBody body;

    public Response() {}
    public Response(Request req) : base("response") {
        this.success = true;
        this.request_seq = req.seq;
        this.command = req.command;
    }

    public void SetBody(ResponseBody bdy) {
        this.success = true;
        this.body = bdy;
    }

    public void SetErrorBody(string msg, ResponseBody bdy = null) {
        this.success = false;
        this.message = msg;
        this.body = bdy;
    }
}
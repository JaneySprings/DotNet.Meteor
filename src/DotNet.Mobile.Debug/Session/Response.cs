namespace DotNet.Mobile.Debug.Session;

public class Response : ProtocolMessage {
    public bool success;
    public string message;
    public int request_seq;
    public string command;
    public ResponseBody body;

    public Response() {
    }
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

/*
* subclasses of ResponseBody are serialized as the body of a response.
* Don't change their instance variables since that will break the debug protocol.
*/
public class ResponseBody { /*empty*/ }

public class ErrorResponseBody : ResponseBody {
    public Message error { get; }

    public ErrorResponseBody(Message error) {
        this.error = error;
    }
}
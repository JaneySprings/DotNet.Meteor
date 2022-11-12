using System.Collections.Generic;
using System.Linq;

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

public class Capabilities : ResponseBody {
    public bool supportsConfigurationDoneRequest;
    public bool supportsFunctionBreakpoints;
    public bool supportsConditionalBreakpoints;
    public bool supportsEvaluateForHovers;
    public dynamic[] exceptionBreakpointFilters;
}

public class StackTraceResponseBody : ResponseBody {
    public StackFrame[] stackFrames { get; }
    public int totalFrames { get; }

    public StackTraceResponseBody(List<StackFrame> frames, int total) {
        stackFrames = frames.ToArray<StackFrame>();
        totalFrames = total;
    }
}

public class ScopesResponseBody : ResponseBody {
    public Scope[] scopes { get; }

    public ScopesResponseBody(List<Scope> scps) {
        scopes = scps.ToArray<Scope>();
    }
}

public class VariablesResponseBody : ResponseBody {
    public Variable[] variables { get; }

    public VariablesResponseBody(List<Variable> vars) {
        variables = vars.ToArray<Variable>();
    }
}

public class ThreadsResponseBody : ResponseBody {
    public Thread[] threads { get; }

    public ThreadsResponseBody(List<Thread> ths) {
        threads = ths.ToArray<Thread>();
    }
}

public class EvaluateResponseBody : ResponseBody {
    public string result { get; }
    public int variablesReference { get; }

    public EvaluateResponseBody(string value, int reff = 0) {
        result = value;
        variablesReference = reff;
    }
}

public class SetBreakpointsResponseBody : ResponseBody {
    public Breakpoint[] breakpoints { get; }

    public SetBreakpointsResponseBody(List<Breakpoint> bpts = null) {
        if (bpts == null)
            breakpoints = new Breakpoint[0];
        else
            breakpoints = bpts.ToArray<Breakpoint>();
    }
}
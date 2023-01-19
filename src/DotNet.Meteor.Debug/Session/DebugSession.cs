using System;
using System.Collections.Generic;
using DotNet.Meteor.Debug.Protocol;

namespace DotNet.Meteor.Debug.Session;

public abstract class DebugSession : Session {
    protected bool clientLinesStartAt1 = true;
    protected bool clientPathsAreURI = true;

    private readonly Dictionary<string, Action<Response, Argument>> requestHandlers;

    protected int ConvertDebuggerLineToClient(int line) => this.clientLinesStartAt1 ? line : line - 1;
    protected int ConvertClientLineToDebugger(int line) => this.clientLinesStartAt1 ? line : line + 1;


    protected DebugSession() {
        requestHandlers = new Dictionary<string, Action<Response, Argument>>() {
            { "initialize", Initialize },
            { "launch", Launch },
            { "attach", Attach },
            { "next", Next },
            { "continue", Continue },
            { "stepIn", StepIn },
            { "stepOut", StepOut },
            { "pause", Pause },
            { "stackTrace", StackTrace },
            { "scopes", Scopes },
            { "variables", Variables },
            { "source", Source },
            { "threads", Threads },
            { "setBreakpoints", SetBreakpoints },
            { "setFunctionBreakpoints", SetFunctionBreakpoints },
            { "setExceptionBreakpoints", SetExceptionBreakpoints },
            { "evaluate", Evaluate },
            { "disconnect", Disconnect }
        };
    }

    protected override void DispatchRequest(string command, Argument args, Response response) {
        try {
            if (requestHandlers.TryGetValue(command, out var handler)) {
                handler.Invoke(response, args);
            } else {
                SendErrorResponse(response, 1014, $"unrecognized request '{command}'");
            }
        } catch (Exception e) {
            var message =
                $"Error occurred while processing {command} request."
                + Environment.NewLine + e.Message + Environment.NewLine;
            SendErrorResponse(response, 1104, message);
            MonoLogger.Instance.LogError("dispatch error", e);
        }
    }

    public abstract void Initialize(Response response, Argument args);
    public abstract void Launch(Response response, Argument arguments);
    public abstract void Attach(Response response, Argument arguments);
    public abstract void Disconnect(Response response, Argument arguments);
    public abstract void SetFunctionBreakpoints(Response response, Argument arguments);
    public abstract void SetExceptionBreakpoints(Response response, Argument arguments);
    public abstract void SetBreakpoints(Response response, Argument arguments);
    public abstract void Continue(Response response, Argument arguments);
    public abstract void Next(Response response, Argument arguments);
    public abstract void StepIn(Response response, Argument arguments);
    public abstract void StepOut(Response response, Argument arguments);
    public abstract void Pause(Response response, Argument arguments);
    public abstract void StackTrace(Response response, Argument arguments);
    public abstract void Scopes(Response response, Argument arguments);
    public abstract void Variables(Response response, Argument arguments);
    public abstract void Source(Response response, Argument arguments);
    public abstract void Threads(Response response, Argument arguments);
    public abstract void Evaluate(Response response, Argument arguments);
}
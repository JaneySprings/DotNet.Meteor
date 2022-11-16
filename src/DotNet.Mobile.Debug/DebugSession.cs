using System;
using DotNet.Mobile.Debug.Protocol;

namespace DotNet.Mobile.Debug {
    public abstract class DebugSession : Session {
        protected bool _clientLinesStartAt1 = true;
        protected bool _clientPathsAreURI = true;

// ----- LifeCycle ------------------------------------------------------------------------------------
        protected override void DispatchRequest(string command, Argument args, Response response) {
            try {
                switch (command) {
                    case "initialize":
                        this._clientLinesStartAt1 = args.LinesStartAt1;
                        var pathFormat = args.PathFormat;

                        if (pathFormat != null) {
                            switch (pathFormat) {
                                case "uri":
                                    this._clientPathsAreURI = true;
                                    break;
                                case "path":
                                    this._clientPathsAreURI = false;
                                    break;
                                default:
                                    SendErrorResponse(response, 1015, $"initialize: bad value '{pathFormat}' for pathFormat");
                                    return;
                            }
                        }
                        Initialize(response, args);
                        break;

                    case "launch":
                        Launch(response, args);
                        break;

                    case "attach":
                        Attach(response, args);
                        break;

                    case "disconnect":
                        Disconnect(response, args);
                        Stop();
                        break;

                    case "next":
                        Next(response, args);
                        break;

                    case "continue":
                        Continue(response, args);
                        break;

                    case "stepIn":
                        StepIn(response, args);
                        break;

                    case "stepOut":
                        StepOut(response, args);
                        break;

                    case "pause":
                        Pause(response, args);
                        break;

                    case "stackTrace":
                        StackTrace(response, args);
                        break;

                    case "scopes":
                        Scopes(response, args);
                        break;

                    case "variables":
                        Variables(response, args);
                        break;

                    case "source":
                        Source(response, args);
                        break;

                    case "threads":
                        Threads(response, args);
                        break;

                    case "setBreakpoints":
                        SetBreakpoints(response, args);
                        break;

                    case "setFunctionBreakpoints":
                        SetFunctionBreakpoints(response, args);
                        break;

                    case "setExceptionBreakpoints":
                        SetExceptionBreakpoints(response, args);
                        break;

                    case "evaluate":
                        Evaluate(response, args);
                        break;

                    default:
                        SendErrorResponse(response, 1014, $"unrecognized command: {command}");
                        break;
                }
            } catch (Exception e) {
                SendErrorResponse(response, 1104, $"error while processing request '{command}' (exception: {e.Message})");
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

// ---------------------------------------------------------------------------------------------

        public void SendResponse(Response response, object body = null) {
            response.SetBody(body);
            SendMessage(response);
        }

        public void SendErrorResponse(Response response, int id, string message) {
            var model = new ModelMessage(id, message);
            response.SetBodyError(message, new BodyError(model));
            SendMessage(response);
        }

        protected int ConvertDebuggerLineToClient(int line) {
            return this._clientLinesStartAt1 ? line : line - 1;
        }

        protected int ConvertClientLineToDebugger(int line) {
            return this._clientLinesStartAt1 ? line : line + 1;
        }
    }
}
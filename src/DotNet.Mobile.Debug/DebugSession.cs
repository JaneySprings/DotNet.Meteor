using System;
using DotNet.Mobile.Debug.Entities;
using DotNet.Mobile.Debug.Protocol;
using DotNet.Mobile.Shared;

namespace DotNet.Mobile.Debug {
    public abstract class DebugSession : ProtocolServer {
        private bool _clientLinesStartAt1 = true;
        private bool _clientPathsAreURI = true;

        public void SendResponse(Response response, dynamic body = null) {
            if (body != null) {
                response.SetBody(body);
            }
            SendMessage(response);
        }

        public void SendErrorResponse(Response response, int id, string format, dynamic arguments = null, bool user = true, bool telemetry = false) {
            var msg = new Message(id, format, arguments, user, telemetry);
            var message = Utilities.ExpandVariables(msg.format, msg.variables);
            response.SetErrorBody(message, new ErrorResponseBody(msg));
            SendMessage(response);
        }

        protected override void DispatchRequest(string command, dynamic args, Response response) {
            if (args == null) {
                args = new { };
            }

            try {
                switch (command) {

                    case "initialize":
                        if (args.linesStartAt1 != null) {
                            this._clientLinesStartAt1 = (bool)args.linesStartAt1;
                        }
                        var pathFormat = (string)args.pathFormat;
                        if (pathFormat != null) {
                            switch (pathFormat) {
                                case "uri":
                                    this._clientPathsAreURI = true;
                                    break;
                                case "path":
                                    this._clientPathsAreURI = false;
                                    break;
                                default:
                                    SendErrorResponse(response, 1015, "initialize: bad value '{_format}' for pathFormat", new { _format = pathFormat });
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
#if !EXCLUDE_HOT_RELOAD
                        if (this.HandleUnknownRequest?.Invoke((command, args, response)) ?? false) {
                            //This was handled!
                            break;
                        }
#endif
                        SendErrorResponse(response, 1014, "unrecognized request: {_request}", new { _request = command });
                        break;
                }
            } catch (Exception e) {
                SendErrorResponse(response, 1104, "error while processing request '{_request}' (exception: {_exception})", new { _request = command, _exception = e.Message });
            }

            if (command == "disconnect") {
                Stop();
            }
        }

        public Func<(string command, dynamic args, Response response), bool> HandleUnknownRequest;

        public abstract void Initialize(Response response, dynamic args);

        public abstract void Launch(Response response, dynamic arguments);

        public abstract void Attach(Response response, dynamic arguments);

        public abstract void Disconnect(Response response, dynamic arguments);

        public virtual void SetFunctionBreakpoints(Response response, dynamic arguments) {
        }

        public virtual void SetExceptionBreakpoints(Response response, dynamic arguments) {
        }

        public abstract void SetBreakpoints(Response response, dynamic arguments);

        public abstract void Continue(Response response, dynamic arguments);

        public abstract void Next(Response response, dynamic arguments);

        public abstract void StepIn(Response response, dynamic arguments);

        public abstract void StepOut(Response response, dynamic arguments);

        public abstract void Pause(Response response, dynamic arguments);

        public abstract void StackTrace(Response response, dynamic arguments);

        public abstract void Scopes(Response response, dynamic arguments);

        public abstract void Variables(Response response, dynamic arguments);

        public abstract void Source(Response response, dynamic arguments);

        public abstract void Threads(Response response, dynamic arguments);

        public abstract void Evaluate(Response response, dynamic arguments);

        // protected

        protected int ConvertDebuggerLineToClient(int line) {
            return this._clientLinesStartAt1 ? line : line - 1;
        }

        protected int ConvertClientLineToDebugger(int line) {
            return this._clientLinesStartAt1 ? line : line + 1;
        }

        protected string ConvertDebuggerPathToClient(string path) {
            if (this._clientPathsAreURI) {
                try {
                    var uri = new System.Uri(path);
                    return uri.AbsoluteUri;
                } catch {
                    return null;
                }
            } else {
                return path;
            }
        }

        protected string ConvertClientPathToDebugger(string clientPath) {
            if (clientPath == null) {
                return null;
            }

            if (this._clientPathsAreURI) {
                if (Uri.IsWellFormedUriString(clientPath, UriKind.Absolute)) {
                    Uri uri = new Uri(clientPath);
                    return uri.LocalPath;
                }
                Logger.Log("path not well formed: '{0}'", clientPath);
                return null;
            } else {
                return clientPath;
            }
        }
    }
}
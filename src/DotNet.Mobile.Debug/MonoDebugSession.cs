/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using System.Net;
using VsCodeMobileUtil;
using Mono.Debugging.Client;
using DotNet.Mobile.Debug.Events;
using DotNet.Mobile.Debug.Protocol;
using DotNet.Mobile.Debug.Pipeline;
using DotNet.Mobile.Shared;

namespace DotNet.Mobile.Debug {
    public class MonoDebugSession : DebugSession {
        private readonly string[] MONO_EXTENSIONS = new String[] {
            ".cs", ".csx",
            ".cake",
            ".fs", ".fsi", ".ml", ".mli", ".fsx", ".fsscript",
            ".hx"
        };
        private const int MAX_CHILDREN = 100;
        private const int MAX_CONNECTION_ATTEMPTS = 20;
        private const int CONNECTION_ATTEMPT_INTERVAL = 500;

        private AutoResetEvent _resumeEvent = new AutoResetEvent(false);
        private bool _debuggeeExecuting = false;
        private readonly object _lock = new object();
        private Mono.Debugging.Soft.SoftDebuggerSession _session;
        private volatile bool _debuggeeKilled = true;
        private ProcessInfo _activeProcess;
        private StackFrame _activeFrame;
        private long _nextBreakpointId = 0;
        private SortedDictionary<long, BreakEvent> _breakpoints;
        private List<Catchpoint> _catchpoints;
        private DebuggerSessionOptions _debuggerSessionOptions;

        private System.Diagnostics.Process _process;
        private Handles<ObjectValue[]> _variableHandles;
        private Handles<StackFrame> _frameHandles;
        private ObjectValue _exception;
        private Dictionary<int, ModelThread> _seenThreads = new();
        private bool _attachMode = false;
        private bool _terminated = false;
        private bool _stderrEOF = true;
        private bool _stdoutEOF = true;

        public MonoDebugSession() : base() {
            this._variableHandles = new Handles<ObjectValue[]>();
            this._frameHandles = new Handles<StackFrame>();
            this._debuggerSessionOptions = new DebuggerSessionOptions {
                EvaluationOptions = EvaluationOptions.DefaultOptions
            };

            this._session = new Mono.Debugging.Soft.SoftDebuggerSession();
            this._session.Breakpoints = new BreakpointStore();

            this._breakpoints = new SortedDictionary<long, BreakEvent>();
            this._catchpoints = new List<Catchpoint>();

            DebuggerLoggingService.CustomLogger = new CustomLogger();

            this._session.ExceptionHandler = ex => {
                return true;
            };

            this._session.LogWriter = (isStdErr, text) => {
            };

            this._session.TargetStopped += (sender, e) => {
                Stopped();
                SendEvent(Event.StoppedEvent, new BodyStopped((int)e.Thread.Id, "step"));
                this._resumeEvent.Set();
            };

            this._session.TargetHitBreakpoint += (sender, e) => {
                Stopped();
                SendEvent(Event.StoppedEvent, new BodyStopped((int)e.Thread.Id, "breakpoint"));
                this._resumeEvent.Set();
            };

            this._session.TargetExceptionThrown += (sender, e) => {
                Stopped();
                var ex = DebuggerActiveException();
                if (ex != null) {
                    this._exception = ex.Instance;
                    SendEvent(Event.StoppedEvent, new BodyStopped((int)e.Thread.Id, "exception", ex.Message));
                }
                this._resumeEvent.Set();
            };

            this._session.TargetUnhandledException += (sender, e) => {
                Stopped();
                var ex = DebuggerActiveException();
                if (ex != null) {
                    this._exception = ex.Instance;
                    SendEvent(Event.StoppedEvent, new BodyStopped((int)e.Thread.Id, "exception", ex.Message));
                }
                this._resumeEvent.Set();
            };

            this._session.TargetStarted += (sender, e) => {
                this._activeFrame = null;
            };

            this._session.TargetReady += (sender, e) => {
                this._activeProcess = this._session.GetProcesses().SingleOrDefault();

                
            };

            this._session.TargetExited += (sender, e) => {

                DebuggerKill();

                this._debuggeeKilled = true;

                Terminate("target exited");

                this._resumeEvent.Set();
            };

            this._session.TargetInterrupted += (sender, e) => {
                this._resumeEvent.Set();
            };

            this._session.TargetEvent += (sender, e) => {
            };

            this._session.TargetThreadStarted += (sender, e) => {
                int tid = (int)e.Thread.Id;
                lock (this._seenThreads) {
                    this._seenThreads[tid] = new ModelThread(tid, e.Thread.Name);
                }
                SendEvent(Event.ThreadEvent, new BodyThread("started", tid));
            };

            this._session.TargetThreadStopped += (sender, e) => {
                int tid = (int)e.Thread.Id;
                lock (this._seenThreads) {
                    this._seenThreads.Remove(tid);
                }
                SendEvent(Event.ThreadEvent, new BodyThread("exited", tid));
            };

            this._session.OutputWriter = (isStdErr, text) => {
                SendOutput(isStdErr ? "stderr" : "stdout", text);
            };
        }

        public override void Initialize(Response response, Argument args) {
            OperatingSystem os = Environment.OSVersion;
            if (os.Platform != PlatformID.MacOSX && os.Platform != PlatformID.Unix && os.Platform != PlatformID.Win32NT) {
                SendErrorResponse(response, 3000, $"Debugging is not supported on this platform ({os.Platform}).");
                return;
            }

            SendResponse(response, new BodyCapabilities() {
                // This debug adapter does not need the configurationDoneRequest.
                SupportsConfigurationDoneRequest = false,
                // This debug adapter does not support function breakpoints.
                SupportsFunctionBreakpoints = false,
                // This debug adapter doesn't support conditional breakpoints.
                SupportsConditionalBreakpoints = false,
                // This debug adapter does not support a side effect free evaluate request for data hovers.
                SupportsEvaluateForHovers = false,
                // This debug adapter does not support exception breakpoint filters
                ExceptionBreakpointFilters = new List<object>()
            });

            // Mono Debug is ready to accept breakpoints immediately
            SendEvent(Event.InitializedEvent, null);
        }

        public override void Launch(Response response, Argument args) {
            this._attachMode = false;

            SetExceptionOptions(args.ExceptionOptions);

            var launchOptions = new LaunchData(args);

            int port = launchOptions.DebugPort; // Utilities.FindFreePort(55555);

            var host = args.Address;
            IPAddress address = string.IsNullOrWhiteSpace(host) ? IPAddress.Loopback : Utilities.ResolveIPAddress(host);
            if (address == null) {
                SendErrorResponse(response, 3013, $"Invalid address '{address}'");
                return;
            }

            if (launchOptions.ProjectType == ProjectType.iOS || launchOptions.ProjectType == ProjectType.MacCatalyst)
                port = Utilities.FindFreePort(55555);

            Connect(launchOptions, address, port);

            SendResponse(response);
        }
        private void Connect(LaunchData options, IPAddress address, int port) {
            lock (this._lock) {
                Logger.Log("Connecting to {0}:{1}", address, port);
                this._debuggeeKilled = false;

                Mono.Debugging.Soft.SoftDebuggerStartArgs args = null;
                if (options.ProjectType == ProjectType.Android) {
                    args = new Mono.Debugging.Soft.SoftDebuggerConnectArgs(options.AppName, address, port) {
                        MaxConnectionAttempts = MAX_CONNECTION_ATTEMPTS,
                        TimeBetweenConnectionAttempts = CONNECTION_ATTEMPT_INTERVAL
                    };
                } else if (options.ProjectType == ProjectType.iOS || options.ProjectType == ProjectType.MacCatalyst) {
                    args = new StreamCommandConnectionDebuggerArgs(options.AppName, new IPhoneTcpCommandConnection(IPAddress.Loopback, port)) { MaxConnectionAttempts = 10 };
                }

                SendConsoleEvent($"Debugger is ready and listening...");

                this._debuggeeExecuting = true;
                this._session.Run(new Mono.Debugging.Soft.SoftDebuggerStartInfo(args), this._debuggerSessionOptions);

            }
        }

        public override void Attach(Response response, Argument args) {
            this._attachMode = true;

            SetExceptionOptions(args.ExceptionOptions);

            // validate argument 'address'
            var host = args.Address;
            if (host == null) {
                SendErrorResponse(response, 3007, "Property 'address' is missing or empty.");
                return;
            }

            // validate argument 'port'
            var port = args.Port;
            if (port == -1) {
                SendErrorResponse(response, 3008, "Property 'port' is missing.");
                return;
            }

            IPAddress address = Utilities.ResolveIPAddress(host);
            if (address == null) {
                SendErrorResponse(response, 3013, $"Invalid address '{address}'");
                return;
            }

            Connect(address, port);

            SendResponse(response);
        }

        public override void Disconnect(Response response, Argument args) {
            if (this._attachMode) {
                lock (this._lock) {
                    if (this._session != null) {
                        this._debuggeeExecuting = true;
                        this._breakpoints.Clear();
                        this._session.Breakpoints.Clear();
                        this._session.Continue();
                        this._session = null;
                    }
                }
            } else {
                // Let's not leave dead Mono processes behind...
                if (this._process != null) {
                    this._process.Kill();
                    this._process = null;
                } else {
                    PauseDebugger();
                    DebuggerKill();

                    while (!this._debuggeeKilled) {
                        Thread.Sleep(10);
                    }
                }
            }

            SendResponse(response);
        }

        public void SendConsoleEvent(string message) {
            Console.WriteLine(message);
            SendEvent(Event.OutputEvent, new BodyOutput(message.TrimEnd() + Environment.NewLine));
        }

        public override void Continue(Response response, Argument args) {
            WaitForSuspend();
            SendResponse(response);
            lock (this._lock) {
                if (this._session != null && !this._session.IsRunning && !this._session.HasExited) {
                    this._session.Continue();
                    this._debuggeeExecuting = true;
                }
            }
        }

        public override void Next(Response response, Argument args) {
            WaitForSuspend();
            SendResponse(response);
            lock (this._lock) {
                if (this._session != null && !this._session.IsRunning && !this._session.HasExited) {
                    this._session.NextLine();
                    this._debuggeeExecuting = true;
                }
            }
        }

        public override void StepIn(Response response, Argument args) {
            WaitForSuspend();
            SendResponse(response);
            lock (this._lock) {
                if (this._session != null && !this._session.IsRunning && !this._session.HasExited) {
                    this._session.StepLine();
                    this._debuggeeExecuting = true;
                }
            }
        }

        public override void StepOut(Response response, Argument args) {
            WaitForSuspend();
            SendResponse(response);
            lock (this._lock) {
                if (this._session != null && !this._session.IsRunning && !this._session.HasExited) {
                    this._session.Finish();
                    this._debuggeeExecuting = true;
                }
            }
        }

        public override void Pause(Response response, Argument args) {
            SendResponse(response);
            PauseDebugger();
        }

        public override void SetExceptionBreakpoints(Response response, Argument args) {
            SetExceptionOptions(args.ExceptionOptions);
            SendResponse(response);
        }
        public override void SetFunctionBreakpoints(Response response, Argument arguments) {}

        public override void SetBreakpoints(Response response, Argument args) {
            string path = null;
            if (args.Source != null) {
                string p = args.Source.Path;
                if (p != null && p.Trim().Length > 0) {
                    path = p;
                }
            }
            if (path == null) {
                SendErrorResponse(response, 3010, "setBreakpoints: property 'source' is empty or misformed");
                return;
            }
            path = path.ConvertClientPathToDebugger(this._clientPathsAreURI);

            if (!HasMonoExtension(path)) {
                // we only support breakpoints in files mono can handle
                SendResponse(response, new BodySetBreakpoints());
                return;
            }

            var clientLines = args.Lines;
            HashSet<int> lin = new HashSet<int>();
            for (int i = 0; i < clientLines.Count; i++) {
                lin.Add(ConvertClientLineToDebugger(clientLines[i]));
            }

            // find all breakpoints for the given path and remember their id and line number
            var bpts = new List<Tuple<int, int>>();
            foreach (var be in this._breakpoints) {
                var bp = be.Value as Breakpoint;
                if (bp != null && bp.FileName == path) {
                    bpts.Add(new Tuple<int, int>((int)be.Key, (int)bp.Line));
                }
            }

            HashSet<int> lin2 = new HashSet<int>();
            foreach (var bpt in bpts) {
                if (lin.Contains(bpt.Item2)) {
                    lin2.Add(bpt.Item2);
                } else {
                    // Program.Log("cleared bpt #{0} for line {1}", bpt.Item1, bpt.Item2);

                    BreakEvent b;
                    if (this._breakpoints.TryGetValue(bpt.Item1, out b)) {
                        this._breakpoints.Remove(bpt.Item1);
                        this._session.Breakpoints.Remove(b);
                    }
                }
            }

            for (int i = 0; i < clientLines.Count; i++) {
                var l = ConvertClientLineToDebugger(clientLines[i]);
                if (!lin2.Contains(l)) {
                    var id = this._nextBreakpointId++;
                    this._breakpoints.Add(id, this._session.Breakpoints.Add(path, l));
                    // Program.Log("added bpt #{0} for line {1}", id, l);
                }
            }

            var breakpoints = new List<ModelBreakpoint>();
            foreach (var l in clientLines) {
                breakpoints.Add(new ModelBreakpoint(true, l));
            }

            SendResponse(response, new BodySetBreakpoints(breakpoints));
        }

        public override void StackTrace(Response response, Argument args) {
            // HOT RELOAD: Seems that sometimes there's a hang here, look out for this in the future
            // TODO: Getting a stack trace can hang; we need to fix it but for now just return an empty one
            //SendResponse(response, new StackTraceResponseBody(new List<StackFrame>(), 0));
            //return;

            int maxLevels = args.Levels;
            int threadReference = args.ThreadId;

            WaitForSuspend();

            ThreadInfo thread = DebuggerActiveThread();
            if (thread.Id != threadReference) {
                // Program.Log("stackTrace: unexpected: active thread should be the one requested");
                thread = FindThread(threadReference);
                if (thread != null) {
                    thread.SetActive();
                }
            }

            var stackFrames = new List<ModelStackFrame>();
            int totalFrames = 0;
            var bt = thread.Backtrace;

            if (bt != null && bt.FrameCount >= 0) {
                totalFrames = bt.FrameCount;

                for (var i = 0; i < Math.Min(totalFrames, maxLevels); i++) {
                    var frame = bt.GetFrame(i);
                    string path = frame.SourceLocation.FileName;
                    var hint = "subtle";

                    ModelSource source = null;
                    if (!string.IsNullOrEmpty(path)) {
                        string sourceName = Path.GetFileName(path);
                        if (!string.IsNullOrEmpty(sourceName)) {
                            if (File.Exists(path)) {
                                source = new ModelSource(sourceName, path.ConvertDebuggerPathToClient(this._clientPathsAreURI), 0, "normal");
                                hint = "normal";
                            } else {
                                source = new ModelSource(sourceName, null, 1000, "deemphasize");
                            }
                        }
                    }

                    var frameHandle = this._frameHandles.Create(frame);
                    string name = frame.SourceLocation.MethodName;
                    int line = frame.SourceLocation.Line;
                    stackFrames.Add(new ModelStackFrame(frameHandle, name, source, ConvertDebuggerLineToClient(line), 0, hint));
                }
            }

            SendResponse(response, new BodyStackTrace(stackFrames, totalFrames));
        }

        public override void Source(Response response, Argument arguments) {
            SendErrorResponse(response, 1020, "No source available");
        }

        public override void Scopes(Response response, Argument args) {

            int frameId = args.FrameId;
            var frame = this._frameHandles.Get(frameId, null);

            var scopes = new List<ModelScope>();

            // TODO: I'm not sure if this is the best response in this scenario but it at least avoids an NRE
            if (frame == null) {
                SendResponse(response, new BodyScopes(scopes));
                return;
            }

            if (frame.Index == 0 && this._exception != null) {
                scopes.Add(new ModelScope("Exception", this._variableHandles.Create(new ObjectValue[] { this._exception })));
            }

            var locals = new[] { frame.GetThisReference() }.Concat(frame.GetParameters()).Concat(frame.GetLocalVariables()).Where(x => x != null).ToArray();
            if (locals.Length > 0) {
                scopes.Add(new ModelScope("Local", this._variableHandles.Create(locals)));
            }

            SendResponse(response, new BodyScopes(scopes));
        }

        public override void Variables(Response response, Argument args) {
            int reference = args.VariablesReference;
            if (reference == -1) {
                SendErrorResponse(response, 3009, "variables: property 'variablesReference' is missing");
                return;
            }

            WaitForSuspend();
            var variables = new List<ModelVariable>();

            ObjectValue[] children;
            if (this._variableHandles.TryGet(reference, out children)) {
                if (children != null && children.Length > 0) {

                    bool more = false;
                    if (children.Length > MAX_CHILDREN) {
                        children = children.Take(MAX_CHILDREN).ToArray();
                        more = true;
                    }

                    if (children.Length < 20) {
                        // Wait for all values at once.
                        WaitHandle.WaitAll(children.Select(x => x.WaitHandle).ToArray());
                        foreach (var v in children) {
                            variables.Add(CreateVariable(v));
                        }
                    } else {
                        foreach (var v in children) {
                            v.WaitHandle.WaitOne();
                            variables.Add(CreateVariable(v));
                        }
                    }

                    if (more) {
                        variables.Add(new ModelVariable("...", null, null));
                    }
                }
            }

            SendResponse(response, new BodyVariables(variables));
        }

        public override void Threads(Response response, Argument args) {
            var threads = new List<ModelThread>();
            var process = this._activeProcess;
            if (process != null) {
                Dictionary<int, ModelThread> d;
                lock (this._seenThreads) {
                    d = new Dictionary<int, ModelThread>(this._seenThreads);
                }
                foreach (var t in process.GetThreads()) {
                    int tid = (int)t.Id;
                    d[tid] = new ModelThread(tid, t.Name);
                }
                threads = d.Values.ToList();
            }
            SendResponse(response, new BodyThreads(threads));
        }

        public override void Evaluate(Response response, Argument args) {
            string error = null;

            var expression = args.Expression;
            if (expression == null) {
                error = "expression missing";
            } else {
                int frameId = args.FrameId;
                var frame = this._frameHandles.Get(frameId, null);
                if (frame != null) {
                    if (frame.ValidateExpression(expression)) {
                        var val = frame.GetExpressionValue(expression, this._debuggerSessionOptions.EvaluationOptions);
                        val.WaitHandle.WaitOne();

                        var flags = val.Flags;
                        if (flags.HasFlag(ObjectValueFlags.Error) || flags.HasFlag(ObjectValueFlags.NotSupported)) {
                            error = val.DisplayValue;
                            if (error.IndexOf("reference not available in the current evaluation context") > 0) {
                                error = "not available";
                            }
                        } else if (flags.HasFlag(ObjectValueFlags.Unknown)) {
                            error = "invalid expression";
                        } else if (flags.HasFlag(ObjectValueFlags.Object) && flags.HasFlag(ObjectValueFlags.Namespace)) {
                            error = "not available";
                        } else {
                            int handle = 0;
                            if (val.HasChildren) {
                                handle = this._variableHandles.Create(val.GetAllChildren());
                            }
                            SendResponse(response, new BodyEvaluate(val.DisplayValue, handle));
                            return;
                        }
                    } else {
                        error = "invalid expression";
                    }
                } else {
                    error = "no active stackframe";
                }
            }
            SendErrorResponse(response, 3014, $"Evaluate request failed ({error}).");
        }

        //---- private ------------------------------------------

        private void SetExceptionOptions(List<ExceptionOption> exceptionOptions) {
            if (exceptionOptions != null) {
                // clear all existig catchpoints
                foreach (var cp in this._catchpoints) {
                    this._session.Breakpoints.Remove(cp);
                }
                this._catchpoints.Clear();

                foreach (var exception in exceptionOptions) {
                    string exName = null;
                    string exBreakMode = exception.BreakMode;

                    if (exception.Path != null) {
                        var path = exception.Path[0];
                        if (path.Names?.Count > 0) {
                            exName = path.Names[0];
                        }
                    }

                    if (exName != null && exBreakMode == "always") {
                        this._catchpoints.Add(this._session.Breakpoints.AddCatchpoint(exName));
                    }
                }
            }
        }

        private void SendOutput(string category, string data) {
            if (!String.IsNullOrEmpty(data)) {
                if (data[data.Length - 1] != '\n') {
                    data += '\n';
                }
                SendEvent(Event.OutputEvent, new BodyOutput(category, data));
            }
        }

        private void Terminate(string reason) {
            if (!this._terminated) {
                // wait until we've seen the end of stdout and stderr
                for (int i = 0; i < 100 && (this._stdoutEOF == false || this._stderrEOF == false); i++) {
                    Thread.Sleep(100);
                }

                SendEvent(Event.TerminatedEvent, null);

                this._terminated = true;
                this._process = null;
            }
        }

        private ThreadInfo FindThread(int threadReference) {
            if (this._activeProcess != null) {
                foreach (var t in this._activeProcess.GetThreads()) {
                    if (t.Id == threadReference) {
                        return t;
                    }
                }
            }
            return null;
        }

        private void Stopped() {
            this._exception = null;
            this._variableHandles.Reset();
            this._frameHandles.Reset();
        }

        private ModelVariable CreateVariable(ObjectValue v) {
            var dv = v.DisplayValue;
            if (dv == null) {
                dv = "<error getting value>";
            }

            if (dv.Length > 1 && dv[0] == '{' && dv[dv.Length - 1] == '}') {
                dv = dv.Substring(1, dv.Length - 2);
            }
            return new ModelVariable(v.Name, dv, v.TypeName, v.HasChildren ? this._variableHandles.Create(v.GetAllChildren()) : 0);
        }

        private bool HasMonoExtension(string path) {
            foreach (var e in this.MONO_EXTENSIONS) {
                if (path.EndsWith(e)) {
                    return true;
                }
            }
            return false;
        }

        //-----------------------

        private void WaitForSuspend() {
            if (this._debuggeeExecuting) {
                this._resumeEvent.WaitOne();
                this._debuggeeExecuting = false;
            }
        }

        private ThreadInfo DebuggerActiveThread() {
            lock (this._lock) {
                return this._session == null ? null : this._session.ActiveThread;
            }
        }

        private Backtrace DebuggerActiveBacktrace() {
            var thr = DebuggerActiveThread();
            return thr == null ? null : thr.Backtrace;
        }

        private ExceptionInfo DebuggerActiveException() {
            var bt = DebuggerActiveBacktrace();
            return bt == null ? null : bt.GetFrame(0).GetException();
        }

        private void Connect(IPAddress address, int port) {
            lock (this._lock) {

                this._debuggeeKilled = false;

                var args0 = new Mono.Debugging.Soft.SoftDebuggerConnectArgs(string.Empty, address, port) {
                    MaxConnectionAttempts = MAX_CONNECTION_ATTEMPTS,
                    TimeBetweenConnectionAttempts = CONNECTION_ATTEMPT_INTERVAL
                };

                this._session.Run(new Mono.Debugging.Soft.SoftDebuggerStartInfo(args0), this._debuggerSessionOptions);

                this._debuggeeExecuting = true;
            }
        }

        private void PauseDebugger() {
            lock (this._lock) {
                if (this._session != null && this._session.IsRunning)
                    this._session.Stop();
            }
        }

        private void DebuggerKill() {
            lock (this._lock) {
                if (this._session != null) {

                    this._debuggeeExecuting = true;

                    if (!this._session.HasExited)
                        this._session.Exit();

                    this._session.Dispose();
                    this._session = null;
                }
            }
        }
    }
}
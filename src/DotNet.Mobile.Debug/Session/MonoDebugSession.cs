using System;
using System.Threading;
using System.Linq;
using System.Net;
using System.IO;
using System.Collections.Generic;
using Mono.Debugging.Soft;
using Mono.Debugging.Client;
using DotNet.Mobile.Shared;
using CThread = System.Threading.Thread;
using MonoFrame = Mono.Debugging.Client.StackFrame;

namespace DotNet.Mobile.Debug.Session;

public class MonoDebugSession: DebugSession {
    private const int MaxChildren = 100;
    private const int MaxConnectionAttempts = 20;
    private const int ConnectionAttemptInterval = 500;

    private AutoResetEvent resumeEvent = new AutoResetEvent(false);
    private SoftDebuggerSession session = new();
    private StackFrame activeFrame;
    private Dictionary<int, Thread> seenThreads = new Dictionary<int, Thread>();
    private List<Catchpoint> catchpoints = new();
    private SortedDictionary<long, BreakEvent> breakpoints = new();
    private Handles<ObjectValue[]> variableHandles = new();
    private Handles<MonoFrame> frameHandles = new();
    private DebuggerSessionOptions debuggerSessionOptions = new() {
         EvaluationOptions = EvaluationOptions.DefaultOptions
    };
    //private HotReloadManager hotReloadManager = new();
    private ObjectValue exception;
    private ProcessInfo activeProcess;
    private readonly object locker = new();
    private bool standardErrorEndFile = true;
    private bool standardOutEndFile = true;
    private bool terminated = false;
    private bool attachMode = false;
    private bool debuggerExecuting = false;
    private volatile bool debuggerKilled = true;
    private long nextBreakpointId = 0;

    public MonoDebugSession(): base() {
        DebuggerLoggingService.CustomLogger = new MonoLogger();

        this.session.Breakpoints = new BreakpointStore();
        this.session.ExceptionHandler = ex => {
            Logger.Info(ex);
            return true;
        };
        this.session.TargetStopped += (sender, e) => {
            Stopped();
            SendEvent(CreateStoppedEvent("step", e.Thread));
            this.resumeEvent.Set();
        };
        this.session.TargetHitBreakpoint += (sender, e) => {
            Stopped();
            SendEvent(CreateStoppedEvent("breakpoint", e.Thread));
            this.resumeEvent.Set();
        };
        this.session.TargetExceptionThrown += (sender, e) => {
            Stopped();
            var ex = DebuggerActiveException();
            if (ex != null) {
                this.exception = ex.Instance;
                SendEvent(CreateStoppedEvent("exception", e.Thread, ex.Message));
            }
            this.resumeEvent.Set();
        };
        this.session.TargetUnhandledException += (sender, e) => {
            Stopped();
            var ex = DebuggerActiveException();
            if (ex != null) {
                this.exception = ex.Instance;
                SendEvent(CreateStoppedEvent("exception", e.Thread, ex.Message));
            }
            this.resumeEvent.Set();
        };
        this.session.TargetStarted += (sender, e) => this.activeFrame = null;
        this.session.TargetReady += (sender, e) => {
            this.activeProcess = this.session.GetProcesses().SingleOrDefault();
            //this._hotReloadManager.Start(this._session);
        };
        this.session.TargetExited += (sender, e) => {
            //this.hotReloadManager?.Stop();
            DebuggerKill();
            this.debuggerKilled = true;

            Terminate("target exited");

            this.resumeEvent.Set();
        };
        this.session.TargetInterrupted += (sender, e) => this.resumeEvent.Set();
        this.session.TargetThreadStarted += (sender, e) => {
            int tid = (int)e.Thread.Id;
            lock (this.seenThreads) {
                this.seenThreads[tid] = new Thread(tid, e.Thread.Name);
            }
            SendEvent(new ThreadEvent("started", tid));
        };
        this.session.TargetThreadStopped += (sender, e) => {
            int tid = (int)e.Thread.Id;
            lock (this.seenThreads) {
                this.seenThreads.Remove(tid);
            }
            SendEvent(new ThreadEvent("exited", tid));
        };
        this.session.OutputWriter = (isStdErr, text) => SendOutput(isStdErr ? "stderr" : "stdout", text);

        // this.HandleUnknownRequest = (s) => {
        //     if (s.command == "DocumentChanged") {
        //         if (this.hotReloadManager == null)
        //             return false;

        //         string fullPath = s.args.fullPath;
        //         string relativePath = s.args.relativePath;
        //         this.hotReloadManager.DocumentChanged(fullPath, relativePath);
        //         return true;
        //     }
        //     return false;
        // };
    }

    // --- DebugSession lifecycle ------------------------------------------------

    public override void Initialize(Response response, dynamic args) {
        OperatingSystem os = Environment.OSVersion;
        if (os.Platform != PlatformID.MacOSX && os.Platform != PlatformID.Unix && os.Platform != PlatformID.Win32NT) {
            SendErrorResponse(response, 3000, "Debugging is not supported on this platform ({_platform}).", new { _platform = os.Platform.ToString() }, true, true);
            return;
        }

        SendResponse(response, new Capabilities() {
            // This debug adapter does not need the configurationDoneRequest.
            supportsConfigurationDoneRequest = false,
            // This debug adapter does not support function breakpoints.
            supportsFunctionBreakpoints = false,
            // This debug adapter doesn't support conditional breakpoints.
            supportsConditionalBreakpoints = false,
            // This debug adapter does not support a side effect free evaluate request for data hovers.
            supportsEvaluateForHovers = false,
            // This debug adapter does not support exception breakpoint filters
            exceptionBreakpointFilters = Array.Empty<dynamic>()
        });

        // Mono Debug is ready to accept breakpoints immediately
        SendEvent(new InitializedEvent());
    }

    public override void Launch(Response response, dynamic args) {
        this.attachMode = false;

        var launchOptions = new LaunchData(args);
        var host = Utils.GetString(args, "address");
        var port = launchOptions.DebugPort;
        //this.hotReloadManager?.SetLaunchData(launchOptions);

        IPAddress address = string.IsNullOrWhiteSpace(host) ? IPAddress.Loopback : Utils.ResolveIPAddress(host);
        if (address == null) {
            SendErrorResponse(response, 3013, "Invalid address '{address}'.", new { address });
            return;
        }

        SetExceptionBreakpoints(args.__exceptionOptions);
        Connect(launchOptions, address, port);

        //on IOS we need to do the connect before we launch the sim.
        if (launchOptions.ProjectType == ProjectType.iOS) {
            //await LaunchiOS(response, launchOptions, port)

            // if (!r.success) {
            //     SendErrorResponse(response, 3002, r.message);
            //     return;
            // }
            SendResponse(response);
            return;
        }

        SendResponse(response);
    }

    private void Connect(LaunchData options, IPAddress address, int port) {
        lock (this.locker) {
            this.debuggerKilled = false;
            SoftDebuggerStartArgs args = null;

            if (options.ProjectType == ProjectType.Android) {
                args = new SoftDebuggerConnectArgs(options.AppName, address, port) {
                    MaxConnectionAttempts = MaxConnectionAttempts,
                    TimeBetweenConnectionAttempts = ConnectionAttemptInterval
                };
            } else if (options.ProjectType == ProjectType.iOS) {
                args = new StreamCommandConnectionDebuggerArgs(options.AppName, new IPhoneTcpCommandConnection(IPAddress.Loopback, port)) {
                    MaxConnectionAttempts = 10
                };
            }

            SendConsoleEvent($"Debugger is ready and listening...");

            this.debuggerExecuting = true;
            this.session.Run(new SoftDebuggerStartInfo(args), this.debuggerSessionOptions);
        }
    }
    private void Connect(IPAddress address, int port) {
        lock (this.locker) {
            this.debuggerKilled = false;
            var args0 = new SoftDebuggerConnectArgs(string.Empty, address, port) {
                MaxConnectionAttempts = MaxConnectionAttempts,
                TimeBetweenConnectionAttempts = ConnectionAttemptInterval
            };

            this.session.Run(new SoftDebuggerStartInfo(args0), this.debuggerSessionOptions);
            this.debuggerExecuting = true;
        }
    }

    public override void Attach(Response response, dynamic args) {
        this.attachMode = true;

        SetExceptionBreakpoints(args.__exceptionOptions);

        // validate argument 'address'
        var host = Utils.GetString(args, "address");
        if (host == null) {
            SendErrorResponse(response, 3007, "Property 'address' is missing or empty.");
            return;
        }

        // validate argument 'port'
        var port = Utils.GetInteger(args, "port", -1);
        if (port == -1) {
            SendErrorResponse(response, 3008, "Property 'port' is missing.");
            return;
        }

        IPAddress address = Utils.ResolveIPAddress(host);
        if (address == null) {
            SendErrorResponse(response, 3013, "Invalid address '{address}'.", new { address });
            return;
        }

        Connect(address, port);
        SendResponse(response);
    }


    public override void Continue(Response response, dynamic args) {
        WaitForSuspend();
        SendResponse(response);
        lock (this.locker) {
            if (this.session?.IsRunning == false && !this.session.HasExited) {
                this.session.Continue();
                this.debuggerExecuting = true;
            }
        }
    }

    public override void Next(Response response, dynamic args) {
        WaitForSuspend();
        SendResponse(response);
        lock (this.locker) {
            if (this.session?.IsRunning == false && !this.session.HasExited) {
                this.session.NextLine();
                this.debuggerExecuting = true;
            }
        }
    }

    public override void StepIn(Response response, dynamic args) {
        WaitForSuspend();
        SendResponse(response);
        lock (this.locker) {
            if (this.session?.IsRunning == false && !this.session.HasExited) {
                this.session.StepLine();
                this.debuggerExecuting = true;
            }
        }
    }

    public override void StepOut(Response response, dynamic args) {
        WaitForSuspend();
        SendResponse(response);
        lock (this.locker) {
            if (this.session?.IsRunning == false && !this.session.HasExited) {
                this.session.Finish();
                this.debuggerExecuting = true;
            }
        }
    }

    public override void Pause(Response response, dynamic args) {
        SendResponse(response);
        PauseDebugger();
    }

    public override void SetBreakpoints(Response response, dynamic args) {
        string path = null;
        if (args.source != null) {
            string p = (string)args.source.path;
            if (p?.Trim().Length > 0) {
                path = p;
            }
        }
        if (path == null) {
            SendErrorResponse(response, 3010, "setBreakpoints: property 'source' is empty or misformed", null, false, true);
            return;
        }
        path = ConvertClientPathToDebugger(path);

        if (!path.HasMonoExtension()) {
            // we only support breakpoints in files mono can handle
            SendResponse(response, new SetBreakpointsResponseBody());
            return;
        }

        var clientLines = args.lines.ToObject<int[]>();
        HashSet<int> lin = new HashSet<int>();
        for (int i = 0; i < clientLines.Length; i++) {
            lin.Add(ConvertClientLineToDebugger(clientLines[i]));
        }

        // find all breakpoints for the given path and remember their id and line number
        var bpts = new List<Tuple<int, int>>();
        foreach (var be in this.breakpoints) {
            if (be.Value is Mono.Debugging.Client.Breakpoint bp && bp.FileName == path) {
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
                if (this.breakpoints.TryGetValue(bpt.Item1, out b)) {
                    this.breakpoints.Remove(bpt.Item1);
                    this.session.Breakpoints.Remove(b);
                }
            }
        }

        for (int i = 0; i < clientLines.Length; i++) {
            var l = ConvertClientLineToDebugger(clientLines[i]);
            if (!lin2.Contains(l)) {
                var id = this.nextBreakpointId++;
                this.breakpoints.Add(id, this.session.Breakpoints.Add(path, l));
                // Program.Log("added bpt #{0} for line {1}", id, l);
            }
        }

        var breakpoints = new List<Breakpoint>();
        foreach (var l in clientLines) {
            breakpoints.Add(new Breakpoint(true, l));
        }

        SendResponse(response, new SetBreakpointsResponseBody(breakpoints));
    }
    public override void SetExceptionBreakpoints(Response response, dynamic args) {
        SetExceptionBreakpoints(args.exceptionOptions);
        SendResponse(response);
    }
    private void SetExceptionBreakpoints(dynamic exceptionOptions) {
        if (exceptionOptions != null) {
            // clear all existig catchpoints
            foreach (var cp in this.catchpoints) {
                this.session.Breakpoints.Remove(cp);
            }
            this.catchpoints.Clear();

            var exceptions = exceptionOptions.ToObject<dynamic[]>();
            for (int i = 0; i < exceptions.Length; i++) {
                var exception = exceptions[i];
                string exName = null;
                string exBreakMode = exception.breakMode;

                if (exception.path != null) {
                    var paths = exception.path.ToObject<dynamic[]>();
                    var path = paths[0];
                    if (path.names != null) {
                        var names = path.names.ToObject<dynamic[]>();
                        if (names.Length > 0) {
                            exName = names[0];
                        }
                    }
                }
                if (exName != null && exBreakMode == "always") {
                    this.catchpoints.Add(this.session.Breakpoints.AddCatchpoint(exName));
                }
            }
        }
    }

    public override void StackTrace(Response response, dynamic args) {
        // HOT RELOAD: Seems that sometimes there's a hang here, look out for this in the future
        // TODO: Getting a stack trace can hang; we need to fix it but for now just return an empty one
        //SendResponse(response, new StackTraceResponseBody(new List<StackFrame>(), 0));
        //return;
        int maxLevels = Utils.GetInteger(args, "levels", 10);
        int threadReference = Utils.GetInteger(args, "threadId", 0);

        WaitForSuspend();

        ThreadInfo thread = DebuggerActiveThread();
        if (thread.Id != threadReference) {
            // Program.Log("stackTrace: unexpected: active thread should be the one requested");
            thread = FindThread(threadReference);
            thread?.SetActive();
        }

        var stackFrames = new List<StackFrame>();
        int totalFrames = 0;

        var bt = thread.Backtrace;
        if (bt?.FrameCount >= 0) {
            totalFrames = bt.FrameCount;

            for (var i = 0; i < Math.Min(totalFrames, maxLevels); i++) {
                var frame = bt.GetFrame(i);
                string path = frame.SourceLocation.FileName;

                var hint = "subtle";
                Source source = null;
                if (!string.IsNullOrEmpty(path)) {
                    string sourceName = Path.GetFileName(path);
                    if (!string.IsNullOrEmpty(sourceName)) {
                        if (File.Exists(path)) {
                            source = new Source(sourceName, ConvertDebuggerPathToClient(path), 0, "normal");
                            hint = "normal";
                        } else {
                            source = new Source(sourceName, null, 1000, "deemphasize");
                        }
                    }
                }

                var frameHandle = this.frameHandles.Create(frame);
                string name = frame.SourceLocation.MethodName;
                int line = frame.SourceLocation.Line;
                stackFrames.Add(new StackFrame(frameHandle, name, source, ConvertDebuggerLineToClient(line), 0, hint));
            }
        }

        SendResponse(response, new StackTraceResponseBody(stackFrames, totalFrames));
    }

    public override void Source(Response response, dynamic arguments) {
        SendErrorResponse(response, 1020, "No source available");
    }

    public override void Scopes(Response response, dynamic args) {

        int frameId = Utils.GetInteger(args, "frameId", 0);
        var frame = this.frameHandles.Get(frameId, null);

        var scopes = new List<Scope>();

        // TODO: I'm not sure if this is the best response in this scenario but it at least avoids an NRE
        if (frame == null) {
            SendResponse(response, new ScopesResponseBody(scopes));
            return;
        }

        if (frame.Index == 0 && this.exception != null) {
            scopes.Add(new Scope("Exception", this.variableHandles.Create(new ObjectValue[] { this.exception })));
        }

        var locals = new[] { frame.GetThisReference() }.Concat(frame.GetParameters()).Concat(frame.GetLocalVariables()).Where(x => x != null).ToArray();
        if (locals.Length > 0) {
            scopes.Add(new Scope("Local", this.variableHandles.Create(locals)));
        }

        SendResponse(response, new ScopesResponseBody(scopes));
    }

    public override void Variables(Response response, dynamic args) {
        int reference = Utils.GetInteger(args, "variablesReference", -1);
        if (reference == -1) {
            SendErrorResponse(response, 3009, "variables: property 'variablesReference' is missing", null, false, true);
            return;
        }

        WaitForSuspend();
        var variables = new List<Variable>();

        ObjectValue[] children;
        if (this.variableHandles.TryGet(reference, out children) && children?.Length > 0) {
            bool more = false;
            if (children.Length > MaxChildren) {
                children = children.Take(MaxChildren).ToArray();
                more = true;
            }

            if (children.Length < 20) {
                // Wait for all values at once.
                WaitHandle.WaitAll(children.Select(x => x.WaitHandle).ToArray());
                foreach (var v in children) {
                    variables.Add(CreateVariable(v));
                }
            }
            else {
                foreach (var v in children) {
                    v.WaitHandle.WaitOne();
                    variables.Add(CreateVariable(v));
                }
            }

            if (more) {
                variables.Add(new Variable("...", null, null));
            }
        }

        SendResponse(response, new VariablesResponseBody(variables));
    }

    public override void Threads(Response response, dynamic args) {
        var threads = new List<Thread>();
        var process = this.activeProcess;
        if (process != null) {
            Dictionary<int, Thread> d;
            lock (this.seenThreads) {
                d = new Dictionary<int, Thread>(this.seenThreads);
            }
            foreach (var t in process.GetThreads()) {
                int tid = (int)t.Id;
                d[tid] = new Thread(tid, t.Name);
            }
            threads = d.Values.ToList();
        }
        SendResponse(response, new ThreadsResponseBody(threads));
    }

    public override void Evaluate(Response response, dynamic args) {
        string error = null;

        var expression = Utils.GetString(args, "expression");
        if (expression == null) {
            error = "expression missing";
        } else {
            int frameId = Utils.GetInteger(args, "frameId", -1);
            var frame = this.frameHandles.Get(frameId, null);
            if (frame != null) {
                if (frame.ValidateExpression(expression)) {
                    var val = frame.GetExpressionValue(expression, this.debuggerSessionOptions.EvaluationOptions);
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
                            handle = this.variableHandles.Create(val.GetAllChildren());
                        }
                        SendResponse(response, new EvaluateResponseBody(val.DisplayValue, handle));
                        return;
                    }
                } else {
                    error = "invalid expression";
                }
            } else {
                error = "no active stackframe";
            }
        }
        SendErrorResponse(response, 3014, "Evaluate request failed ({_reason}).", new { _reason = error });
    }


    private void Stopped() {
        this.exception = null;
        this.variableHandles.Reset();
        this.frameHandles.Reset();
    }

    public override void Disconnect(Response response, dynamic args) {
        // try {
        //     if (!(this.iOSDebuggerProcess?.HasExited ?? true)) {
        //         SendConsoleEvent($"Stopping iOS process...");

        //         this.iOSDebuggerProcess?.StandardInput?.WriteLine("\r\n");
        //         this.iOSDebuggerProcess?.Kill();
        //         this.iOSDebuggerProcess = null;

        //         SendConsoleEvent($"iOS Process was stopped.");
        //     }
        // } catch (Exception ex) {
        //     Console.WriteLine(ex);
        // }

        if (this.attachMode) {
            lock (this.locker) {
                if (this.session != null) {
                    this.debuggerExecuting = true;
                    this.breakpoints.Clear();
                    this.session.Breakpoints.Clear();
                    this.session.Continue();
                    this.session = null;
                }
            }
        } else {
            PauseDebugger();
            DebuggerKill();

            while (!this.debuggerKilled) {
                CThread.Sleep(10);
            }
        }

        SendResponse(response);
    }

    private void PauseDebugger() {
        lock (this.locker) {
            if (this.session?.IsRunning == true)
                this.session.Stop();
        }
    }

    private void DebuggerKill() {
        lock (this.locker) {
            if (this.session != null) {
                this.debuggerExecuting = true;

                if (!this.session.HasExited)
                    this.session.Exit();

                this.session.Dispose();
                this.session = null;
            }
        }
    }

    private void Terminate(string reason) {
        Logger.Info($"Terminating: {reason}");
        if (!this.terminated) {
            // wait until we've seen the end of stdout and stderr
            for (int i = 0; i < 100 && (!this.standardErrorEndFile || !this.standardOutEndFile); i++) {
                CThread.Sleep(100);
            }
            SendEvent(new TerminatedEvent());
            this.terminated = true;
        }
    }

    // --- DebugSession utils ----------------------------------------------------

    private ThreadInfo DebuggerActiveThread() {
        lock (this.locker) {
            return this.session?.ActiveThread;
        }
    }
    private Backtrace DebuggerActiveBacktrace() {
        var thr = DebuggerActiveThread();
        return thr?.Backtrace;
    }
    private ExceptionInfo DebuggerActiveException() {
        var bt = DebuggerActiveBacktrace();
        return bt?.GetFrame(0).GetException();
    }

    private void SendOutput(string category, string data) {
        if (!string.IsNullOrEmpty(data)) {
            if (data[data.Length - 1] != '\n') {
                data += '\n';
            }
            SendEvent(new OutputEvent(category, data));
        }
    }

    private void WaitForSuspend() {
        if (this.debuggerExecuting) {
            this.resumeEvent.WaitOne();
            this.debuggerExecuting = false;
        }
    }

    public static StoppedEvent CreateStoppedEvent(string reason, ThreadInfo ti, string text = null) {
        return new StoppedEvent((int)ti.Id, reason, text);
    }
    public void SendConsoleEvent(string message) {
        Logger.Info(message);
        SendEvent(new ConsoleOutputEvent(message.TrimEnd() + Environment.NewLine));
    }

    private ThreadInfo FindThread(int threadReference) {
        if (this.activeProcess != null) {
            foreach (var t in this.activeProcess.GetThreads()) {
                if (t.Id == threadReference) {
                    return t;
                }
            }
        }
        return null;
    }

     private Variable CreateVariable(ObjectValue v) {
        var dv = v.DisplayValue;
        if (dv == null) {
            dv = "<error getting value>";
        }

        if (dv.Length > 1 && dv[0] == '{' && dv[dv.Length - 1] == '}') {
            dv = dv.Substring(1, dv.Length - 2);
        }
        return new Variable(v.Name, dv, v.TypeName, v.HasChildren ? this.variableHandles.Create(v.GetAllChildren()) : 0);
    }
}
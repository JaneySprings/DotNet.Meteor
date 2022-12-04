﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using System.Net;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;
using DotNet.Mobile.Debug.Events;
using DotNet.Mobile.Debug.Protocol;
using DotNet.Mobile.Debug.Pipeline;
using XCode.Sdk;
using Process = System.Diagnostics.Process;

namespace DotNet.Mobile.Debug.Session;

public class MonoDebugSession : DebugSession {
    private const int MAX_CHILDREN = 100;
    private const int MAX_CONNECTION_ATTEMPTS = 20;
    private const int CONNECTION_ATTEMPT_INTERVAL = 500;

    private bool terminated;
    private long nextBreakpointId;
    private bool debuggerExecuting;
    private volatile bool debuggerKilled = true;
    private readonly object locker = new object();

    private Process process;
    private ObjectValue exception;
    private ProcessInfo activeProcess;
    private readonly AutoResetEvent resumeEvent = new AutoResetEvent(false);
    private readonly List<Catchpoint> catchpoints = new List<Catchpoint>();
    private readonly Handles<StackFrame> frameHandles = new Handles<StackFrame>();
    private readonly Handles<ObjectValue[]> variableHandles = new Handles<ObjectValue[]>();
    private readonly Dictionary<int, ModelThread> seenThreads = new Dictionary<int, ModelThread>();
    private readonly SortedDictionary<long, BreakEvent> breakpoints = new SortedDictionary<long, BreakEvent>();
    private readonly DebuggerSessionOptions debuggerSessionOptions = new DebuggerSessionOptions {
        EvaluationOptions = EvaluationOptions.DefaultOptions
    };
    private SoftDebuggerSession session = new SoftDebuggerSession {
        Breakpoints = new BreakpointStore()
    };


    public MonoDebugSession() : base() {
        DebuggerLoggingService.CustomLogger = new MonoLogger();

        this.session.ExceptionHandler = ex => {
            Logger.Log(ex);
            return true;
        };
        this.session.LogWriter = (isStdErr, text) => Logger.Log(text);
        this.session.TargetStopped += (sender, e) => {
            Stopped();
            SendEvent(Event.StoppedEvent, new BodyStopped((int)e.Thread.Id, "step"));
            this.resumeEvent.Set();
        };
        this.session.TargetHitBreakpoint += (sender, e) => {
            Stopped();
            SendEvent(Event.StoppedEvent, new BodyStopped((int)e.Thread.Id, "breakpoint"));
            this.resumeEvent.Set();
        };
        this.session.TargetExceptionThrown += (sender, e) => {
            Stopped();
            var ex = DebuggerActiveException();
            if (ex != null) {
                this.exception = ex.Instance;
                SendEvent(Event.StoppedEvent, new BodyStopped((int)e.Thread.Id, "exception", ex.Message));
            }
            this.resumeEvent.Set();
        };
        this.session.TargetUnhandledException += (sender, e) => {
            Stopped();
            var ex = DebuggerActiveException();
            if (ex != null) {
                this.exception = ex.Instance;
                SendEvent(Event.StoppedEvent, new BodyStopped((int)e.Thread.Id, "exception", ex.Message));
            }
            this.resumeEvent.Set();
        };
        this.session.TargetReady += (sender, e) => this.activeProcess = this.session.GetProcesses().SingleOrDefault();
        this.session.TargetExited += (sender, e) => {
            DebuggerKill();
            this.debuggerKilled = true;
            Terminate("target exited");
            this.resumeEvent.Set();
        };
        this.session.TargetInterrupted += (sender, e) => this.resumeEvent.Set();
        this.session.TargetEvent += (sender, e) => {};
        this.session.TargetThreadStarted += (sender, e) => {
            int tid = (int)e.Thread.Id;
            lock (this.seenThreads) {
                this.seenThreads[tid] = new ModelThread(tid, e.Thread.Name);
            }
            SendEvent(Event.ThreadEvent, new BodyThread("started", tid));
        };
        this.session.TargetThreadStopped += (sender, e) => {
            int tid = (int)e.Thread.Id;
            lock (this.seenThreads) {
                this.seenThreads.Remove(tid);
            }
            SendEvent(Event.ThreadEvent, new BodyThread("exited", tid));
        };
        this.session.OutputWriter = (isStdErr, text) => SendOutput(isStdErr ? "stderr" : "stdout", text);
    }

    public override void Initialize(Response response, Argument args) {
        OperatingSystem os = Environment.OSVersion;
        if (os.Platform != PlatformID.MacOSX && os.Platform != PlatformID.Unix && os.Platform != PlatformID.Win32NT) {
            SendErrorResponse(response, 3000, $"Debugging is not supported on this platform ({os.Platform}).");
            return;
        }

        this.clientLinesStartAt1 = args.LinesStartAt1;
        var pathFormat = args.PathFormat;

        if (pathFormat != null) {
            switch (pathFormat) {
                case "uri":
                    this.clientPathsAreURI = true;
                    break;
                case "path":
                    this.clientPathsAreURI = false;
                    break;
                default:
                    SendErrorResponse(response, 1015, $"initialize: bad value '{pathFormat}' for pathFormat");
                    return;
            }
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
        SetExceptionOptions(args.ExceptionOptions);

        var launchOptions = new LaunchData(args.Project, args.Device, args.Target);

        int port = args.DebuggingPort;
        var host = args.Address;

        IPAddress address = string.IsNullOrWhiteSpace(host) ? IPAddress.Loopback : Utilities.ResolveIPAddress(host);
        if (address == null) {
            SendErrorResponse(response, 3013, $"Invalid address '{address}'");
            return;
        }

        Connect(launchOptions, address, port);

        if (launchOptions.Platform == Platform.iOS) {
            MLaunch.LaunchAppForDebug(launchOptions.BundlePath, launchOptions.Device, port);
        }

        SendResponse(response);
    }
    private void Connect(LaunchData options, IPAddress address, int port) {
        lock (this.locker) {
            this.debuggerKilled = false;

            if (options.Target.Equals("Release", StringComparison.OrdinalIgnoreCase))
                return;

            SoftDebuggerStartArgs args = null;
            if (options.Platform == Platform.Android) {
                args = new SoftDebuggerConnectArgs(options.AppName, address, port) {
                    MaxConnectionAttempts = MAX_CONNECTION_ATTEMPTS,
                    TimeBetweenConnectionAttempts = CONNECTION_ATTEMPT_INTERVAL
                };
            } else if (options.Platform == Platform.iOS) {
                args = new StreamCommandConnectionDebuggerArgs(options.AppName, new IPhoneTcpCommandConnection(IPAddress.Loopback, port)) { MaxConnectionAttempts = 10 };
            }

            SendEvent(Event.OutputEvent, new BodyOutput("Debugger is ready and listening..." + Environment.NewLine));
            Logger.Log("Debugger ready at {0}:{1}", address, port);

            this.debuggerExecuting = true;
            this.session.Run(new SoftDebuggerStartInfo(args), this.debuggerSessionOptions);
        }
    }

    public override void Attach(Response response, Argument args) {
        SendErrorResponse(response, 3008, "Attach is not supported yet");
    }

    public override void Disconnect(Response response, Argument args) {
        // Let's not leave dead Mono processes behind...
        if (this.process != null) {
            this.process.Kill();
            this.process = null;
        } else {
            PauseDebugger();
            DebuggerKill();

            while (!this.debuggerKilled) {
                Thread.Sleep(10);
            }
        }

        SendResponse(response);
        Stop();
    }

    public override void Continue(Response response, Argument args) {
        WaitForSuspend();
        SendResponse(response);
        lock (this.locker) {
            if (this.session?.IsRunning == false && !this.session.HasExited) {
                this.session.Continue();
                this.debuggerExecuting = true;
            }
        }
    }

    public override void Next(Response response, Argument args) {
        WaitForSuspend();
        SendResponse(response);
        lock (this.locker) {
            if (this.session?.IsRunning == false && !this.session.HasExited) {
                this.session.NextLine();
                this.debuggerExecuting = true;
            }
        }
    }

    public override void StepIn(Response response, Argument args) {
        WaitForSuspend();
        SendResponse(response);
        lock (this.locker) {
            if (this.session?.IsRunning == false && !this.session.HasExited) {
                this.session.StepLine();
                this.debuggerExecuting = true;
            }
        }
    }

    public override void StepOut(Response response, Argument args) {
        WaitForSuspend();
        SendResponse(response);
        lock (this.locker) {
            if (this.session?.IsRunning == false && !this.session.HasExited) {
                this.session.Finish();
                this.debuggerExecuting = true;
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
        path = path.ConvertClientPathToDebugger(this.clientPathsAreURI);

        if (!path.HasMonoExtension()) {
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
        foreach (var be in this.breakpoints) {
            if (be.Value is Breakpoint bp && bp.FileName == path) {
                bpts.Add(new Tuple<int, int>((int)be.Key, (int)bp.Line));
            }
        }

        HashSet<int> lin2 = new HashSet<int>();
        foreach (var bpt in bpts) {
            if (lin.Contains(bpt.Item2)) {
                lin2.Add(bpt.Item2);
            } else {
                if (this.breakpoints.TryGetValue(bpt.Item1, out BreakEvent b)) {
                    this.breakpoints.Remove(bpt.Item1);
                    this.session.Breakpoints.Remove(b);
                }
            }
        }

        for (int i = 0; i < clientLines.Count; i++) {
            var l = ConvertClientLineToDebugger(clientLines[i]);
            if (!lin2.Contains(l)) {
                var id = this.nextBreakpointId++;
                this.breakpoints.Add(id, this.session.Breakpoints.Add(path, l));
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
            thread = FindThread(threadReference);
            thread?.SetActive();
        }

        var stackFrames = new List<ModelStackFrame>();
        int totalFrames = 0;
        var bt = thread.Backtrace;

        if (bt?.FrameCount >= 0) {
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
                            source = new ModelSource(sourceName, path.ConvertDebuggerPathToClient(this.clientPathsAreURI), 0, "normal");
                            hint = "normal";
                        } else {
                            source = new ModelSource(sourceName, null, 1000, "deemphasize");
                        }
                    }
                }

                var frameHandle = this.frameHandles.Create(frame);
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
        var frame = this.frameHandles.Get(frameId, null);

        var scopes = new List<ModelScope>();

        // TODO: I'm not sure if this is the best response in this scenario but it at least avoids an NRE
        if (frame == null) {
            SendResponse(response, new BodyScopes(scopes));
            return;
        }

        if (frame.Index == 0 && this.exception != null) {
            scopes.Add(new ModelScope("Exception", this.variableHandles.Create(new ObjectValue[] { this.exception })));
        }

        var locals = new[] { frame.GetThisReference() }.Concat(frame.GetParameters()).Concat(frame.GetLocalVariables()).Where(x => x != null).ToArray();
        if (locals.Length > 0) {
            scopes.Add(new ModelScope("Local", this.variableHandles.Create(locals)));
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

        if (this.variableHandles.TryGet(reference, out ObjectValue[] children) && children?.Length > 0) {
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
            }
            else {
                foreach (var v in children) {
                    v.WaitHandle.WaitOne();
                    variables.Add(CreateVariable(v));
                }
            }

            if (more) {
                variables.Add(new ModelVariable("...", null, null));
            }
        }

        SendResponse(response, new BodyVariables(variables));
    }

    public override void Threads(Response response, Argument args) {
        var threads = new List<ModelThread>();
        var process = this.activeProcess;
        if (process != null) {
            Dictionary<int, ModelThread> d;
            lock (this.seenThreads) {
                d = new Dictionary<int, ModelThread>(this.seenThreads);
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
            foreach (var cp in this.catchpoints) {
                this.session.Breakpoints.Remove(cp);
            }
            this.catchpoints.Clear();

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
                    this.catchpoints.Add(this.session.Breakpoints.AddCatchpoint(exName));
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
        Logger.Log("Terminating debugger: " + reason);
        if (!this.terminated) {
            // wait until we've seen the end of stdout and stderr
            for (int i = 0; i < 100; i++) {
                Thread.Sleep(100);
            }

            SendEvent(Event.TerminatedEvent, null);

            this.terminated = true;
            this.process = null;
        }
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

    private void Stopped() {
        this.exception = null;
        this.variableHandles.Reset();
        this.frameHandles.Reset();
    }

    private ModelVariable CreateVariable(ObjectValue v) {
        var dv = v.DisplayValue ?? "<error getting value>";

        if (dv.Length > 1 && dv[0] == '{' && dv[dv.Length - 1] == '}')
            dv = dv.Substring(1, dv.Length - 2);

        return new ModelVariable(v.Name, dv, v.TypeName, v.HasChildren ? this.variableHandles.Create(v.GetAllChildren()) : 0);
    }

    private void WaitForSuspend() {
        if (this.debuggerExecuting) {
            this.resumeEvent.WaitOne();
            this.debuggerExecuting = false;
        }
    }

    private ThreadInfo DebuggerActiveThread() {
        lock (this.locker) {
            return this.session.ActiveThread;
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
}
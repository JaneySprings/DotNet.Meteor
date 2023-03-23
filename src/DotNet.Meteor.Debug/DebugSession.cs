using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using Mono.Debugging.Soft;
using DotNet.Meteor.Debug.Utilities;
using DebugProtocol = DotNet.Meteor.Debug.Protocol;
using MonoClient = Mono.Debugging.Client;
using Process = System.Diagnostics.Process;

namespace DotNet.Meteor.Debug;

public partial class DebugSession : Session {
    private bool terminated;
    private bool debuggerExecuting;
    private readonly object locker = new object();

    private MonoClient.ObjectValue exception;
    private MonoClient.ProcessInfo activeProcess;
    private readonly List<Process> processes = new List<Process>();
    private readonly AutoResetEvent resumeEvent = new AutoResetEvent(false);
    private readonly Handles<MonoClient.StackFrame> frameHandles = new Handles<MonoClient.StackFrame>();
    private readonly Handles<MonoClient.ObjectValue[]> variableHandles = new Handles<MonoClient.ObjectValue[]>();
    private readonly Dictionary<int, DebugProtocol.Types.Thread> seenThreads = new Dictionary<int, DebugProtocol.Types.Thread>();
    private SoftDebuggerSession session = new SoftDebuggerSession {
        Breakpoints = new MonoClient.BreakpointStore()
    };
    private readonly MonoClient.DebuggerSessionOptions sessionOptions = new MonoClient.DebuggerSessionOptions {
        EvaluationOptions = new MonoClient.EvaluationOptions {
            EvaluationTimeout = 5000,
            MemberEvaluationTimeout = 5000,
            UseExternalTypeResolver = true,
            AllowMethodEvaluation = true,
            GroupPrivateMembers = true,
            GroupStaticMembers = true,
            AllowToStringCalls = true,
            AllowTargetInvoke = true,
            ChunkRawStrings = false,
            CurrentExceptionTag = "$exception",
            IntegerDisplayFormat = MonoClient.IntegerDisplayFormat.Decimal,
            StackFrameFormat = new MonoClient.StackFrameFormat()
        }
    };

    public DebugSession() {
        MonoClient.DebuggerLoggingService.CustomLogger = new MonoLogger();

        this.session.LogWriter = OnSessionLog;
        this.session.DebugWriter = OnDebugLog;
        this.session.OutputWriter = OnLog;

        this.session.ExceptionHandler = OnExceptionHandled;

        this.session.TargetStopped += TargetStopped;
        this.session.TargetHitBreakpoint += TargetHitBreakpoint;
        this.session.TargetExceptionThrown += TargetExceptionThrown;
        this.session.TargetUnhandledException += TargetExceptionThrown;
        this.session.TargetReady += TargetReady;
        this.session.TargetExited += TargetExited;
        this.session.TargetInterrupted += TargetInterrupted;
        this.session.TargetThreadStarted += TargetThreadStarted;
        this.session.TargetThreadStopped += TargetThreadStopped;
    }

#region request: Initialize
    protected override void Initialize(DebugProtocol.Response response, DebugProtocol.Arguments args) {
        response.SetSuccess(new DebugProtocol.Capabilities {
            SupportsEvaluateForHovers = true,
            SupportsExceptionInfoRequest = true,
            SupportsConditionalBreakpoints = true,
            SupportsHitConditionalBreakpoints = true,
            SupportsLogPoints = true,
            SupportsExceptionOptions = true,
            SupportsExceptionFilterOptions = true,
            ExceptionBreakpointFilters = new List<DebugProtocol.Types.ExceptionBreakpointsFilter> {
                DebugProtocol.Types.ExceptionBreakpointsFilter.AllExceptions,
            }
        });
    }
#endregion
#region request: Launch
    protected override void Launch(DebugProtocol.Response response, DebugProtocol.Arguments args) {
        var configuration = new LaunchData(args.Project, args.Device, args.Target);
        var sourceLocation = Path.Combine(Path.GetDirectoryName(configuration.Project.Path), ".meteor", "sources");
        var port = args.DebuggingPort == 0 ? Extensions.FindFreePort() : args.DebuggingPort;

        this.session.SetSourceLocation(sourceLocation);

        if (port < 1) {
            response.SetError($"Invalid port '{port}'");
            return;
        }

        response.SetSuccess();
        LaunchApplication(configuration, port, this.processes);
        Connect(configuration, port);
    }
#endregion
#region request: Disconnect
    protected override void Disconnect(DebugProtocol.Response response, DebugProtocol.Arguments args) {
        lock (this.locker) {
            if (this.session?.IsRunning == true)
                this.session.Stop();
        }
        KillDebugger();
        response.SetSuccess();
        StopGlobalLoop();
    }
#endregion
#region request: Continue
    protected override void Continue(DebugProtocol.Response response, DebugProtocol.Arguments args) {
        WaitForSuspend();
        response.SetSuccess();
        lock (this.locker) {
            if (this.session?.IsRunning == false && !this.session.HasExited) {
                this.session.Continue();
                this.debuggerExecuting = true;
            }
        }
    }
#endregion
#region request: Next
    protected override void Next(DebugProtocol.Response response, DebugProtocol.Arguments args) {
        WaitForSuspend();
        response.SetSuccess();
        lock (this.locker) {
            if (this.session?.IsRunning == false && !this.session.HasExited) {
                this.session.NextLine();
                this.debuggerExecuting = true;
            }
        }
    }
#endregion
#region request: StepIn
    protected override void StepIn(DebugProtocol.Response response, DebugProtocol.Arguments args) {
        WaitForSuspend();
        response.SetSuccess();
        lock (this.locker) {
            if (this.session?.IsRunning == false && !this.session.HasExited) {
                this.session.StepLine();
                this.debuggerExecuting = true;
            }
        }
    }
#endregion
#region request: StepOut
    protected override void StepOut(DebugProtocol.Response response, DebugProtocol.Arguments args) {
        WaitForSuspend();
        response.SetSuccess();
        lock (this.locker) {
            if (this.session?.IsRunning == false && !this.session.HasExited) {
                this.session.Finish();
                this.debuggerExecuting = true;
            }
        }
    }
#endregion
#region request: Pause
    protected override void Pause(DebugProtocol.Response response, DebugProtocol.Arguments args) {
        response.SetSuccess();
        lock (this.locker) {
            if (this.session?.IsRunning == true)
                this.session.Stop();
        }
    }
#endregion
#region request: SetExceptionBreakpoints
    protected override void SetExceptionBreakpoints(DebugProtocol.Response response, DebugProtocol.Arguments args) {
        if (args.FilterOptions == null || args.FilterOptions.Count == 0) {
            this.session.Breakpoints.ClearCatchpoints();
            response.SetSuccess();
            return;
        }

        foreach (var option in args.FilterOptions) {
            if (option.FilterId == DebugProtocol.Types.ExceptionBreakpointsFilter.AllExceptions.Filter) {
                var exceptionFilter = typeof(Exception).ToString();

                if (!string.IsNullOrEmpty(option.Condition))
                    exceptionFilter = option.Condition;

                this.session.Breakpoints.ClearCatchpoints();
                this.session.Breakpoints.AddCatchpoint(exceptionFilter);
            }
        }

        response.SetSuccess();
    }
#endregion
#region request: SetBreakpoints
    protected override void SetBreakpoints(DebugProtocol.Response response, DebugProtocol.Arguments args) {
        var breakpoints = new List<DebugProtocol.Types.Breakpoint>();
        var breakpointsInfos = args.Breakpoints;
        var sourcePath = args.Source?.Path;

        // Remove unexisting breakpoints
        var fileBreakpoints = this.session.Breakpoints.GetBreakpointsAtFile(sourcePath);
        foreach(var fileBreakpoint in fileBreakpoints) {
            if (breakpointsInfos.Find(b => b.Line == fileBreakpoint.Line) == null) {
                this.session.Breakpoints.Remove(fileBreakpoint);
            }
        }
        // Add new breakpoints
        foreach(var breakpointInfo in breakpointsInfos) {
            MonoClient.Breakpoint breakpoint = this.session.Breakpoints.Add(sourcePath, breakpointInfo.Line, breakpointInfo.Column ?? 1);
            // Conditional breakpoint
            if (breakpoint != null && breakpointInfo.Condition != null)
                breakpoint.ConditionExpression = breakpointInfo.Condition;
            // Hit count breakpoint
            if (breakpoint != null && !string.IsNullOrEmpty(breakpointInfo.HitCondition)) {
                breakpoint.HitCountMode = MonoClient.HitCountMode.EqualTo;
                breakpoint.HitCount = int.TryParse(breakpointInfo.HitCondition, out int hitCount) ? hitCount : 1;
            }
            // Logpoint
            if (breakpoint != null && breakpointInfo.LogMessage != null) {
                breakpoint.HitAction = MonoClient.HitAction.PrintExpression;
                breakpoint.TraceExpression = $"LogPoint: {breakpointInfo.LogMessage}";
            }

            breakpoints.Add(new DebugProtocol.Types.Breakpoint(
                breakpoint != null,
                breakpoint?.Line ?? breakpointInfo.Line,
                breakpoint?.Column ?? breakpointInfo.Column
            ));
        }

        response.SetSuccess(new DebugProtocol.SetBreakpointsResponseBody(breakpoints));
    }
#endregion
#region request: StackTrace
    protected override void StackTrace(DebugProtocol.Response response, DebugProtocol.Arguments args) {
        WaitForSuspend();

        MonoClient.ThreadInfo thread = DebuggerActiveThread();
        if (thread.Id != args.ThreadId) {
            thread = FindThread(args.ThreadId);
            thread?.SetActive();
        }

        var stackFrames = new List<DebugProtocol.Types.StackFrame>();
        var bt = thread.Backtrace;

        if (bt?.FrameCount < 0) {
            response.SetError("No stack trace available");
            return;
        }

        int totalFrames = bt.FrameCount;
        for (var i = args.StartFrame; i < Math.Min(args.StartFrame + args.Levels, totalFrames); i++) {
            DebugProtocol.Types.Source source = null;
            var frame = bt.GetFrame(i);
            var sourceLocation = frame.SourceLocation;
            string sourceName = string.Empty;
            string hint = string.Empty;

            if (!string.IsNullOrEmpty(sourceLocation.FileName)) {
                sourceName = Path.GetFileName(sourceLocation.FileName);
                if (File.Exists(sourceLocation.FileName)) {
                    var path = sourceLocation.FileName;
                    source = new DebugProtocol.Types.Source(sourceName, path, 0, "normal");
                    hint = "normal";
                }
            }
            if (source == null) {
                source = new DebugProtocol.Types.Source(sourceName, null, 1000, "deemphasize");
                hint = "subtle";
            }

            stackFrames.Add(new DebugProtocol.Types.StackFrame(
                this.frameHandles.Create(frame), source, hint,
                frame.SourceLocation.MethodName,
                frame.SourceLocation.Line,
                frame.SourceLocation.Column,
                frame.SourceLocation.EndLine,
                frame.SourceLocation.EndColumn
            ));
        }
        response.SetSuccess(new DebugProtocol.StackTraceResponseBody(stackFrames, totalFrames));
    }
#endregion
#region request: Scopes
    protected override void Scopes(DebugProtocol.Response response, DebugProtocol.Arguments args) {
        int frameId = args.FrameId;
        var frame = this.frameHandles.Get(frameId, null);
        var scopes = new List<DebugProtocol.Types.Scope>();

        if (frame == null) {
            response.SetError("frame not found");
            return;
        }

        if (this.exception != null)
            scopes.Add(new DebugProtocol.Types.Scope("Exception", this.variableHandles.Create(new MonoClient.ObjectValue[] { this.exception })));

        scopes.Add(new DebugProtocol.Types.Scope("Local", this.variableHandles.Create(frame.GetAllLocals())));
        response.SetSuccess(new DebugProtocol.ScopesResponseBody(scopes));
    }
#endregion
#region request: Variables
    protected override void Variables(DebugProtocol.Response response, DebugProtocol.Arguments args) {
        int reference = args.VariablesReference;
        if (reference == -1) {
            response.SetError("variables: property 'variablesReference' is missing");
            return;
        }

        WaitForSuspend();
        var variables = new List<DebugProtocol.Types.Variable>();

        if (this.variableHandles.TryGet(reference, out MonoClient.ObjectValue[] children) && children?.Length > 0) {
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
        }
        response.SetSuccess(new DebugProtocol.VariablesResponseBody(variables));
    }
#endregion
#region request: Threads
    protected override void Threads(DebugProtocol.Response response, DebugProtocol.Arguments args) {
        var threads = new List<DebugProtocol.Types.Thread>();
        var process = this.activeProcess;
        if (process != null) {
            Dictionary<int, DebugProtocol.Types.Thread> d;
            lock (this.seenThreads) {
                d = new Dictionary<int, DebugProtocol.Types.Thread>(this.seenThreads);
            }
            foreach (var t in process.GetThreads()) {
                int tid = (int)t.Id;
                d[tid] = new DebugProtocol.Types.Thread(tid, t.Name);
            }
            threads = d.Values.ToList();
        }
        response.SetSuccess(new DebugProtocol.ThreadsResponseBody(threads));
    }
#endregion
#region request: Evaluate
    protected override void Evaluate(DebugProtocol.Response response, DebugProtocol.Arguments args) {
        if (string.IsNullOrEmpty(args.Expression)) {
            response.SetError("expression missing");
            return;
        }
        var frame = this.frameHandles.Get(args.FrameId, null);
        if (frame == null) {
            response.SetError("no active stackframe");
            return;
        }
        if (!frame.ValidateExpression(args.Expression)) {
            response.SetError("invalid expression");
            return;
        }

        var val = frame.GetExpressionValue(args.Expression, this.sessionOptions.EvaluationOptions);
        val.WaitHandle.WaitOne();

        if (val.Flags.HasFlag(MonoClient.ObjectValueFlags.Error) || val.Flags.HasFlag(MonoClient.ObjectValueFlags.NotSupported)) {
            response.SetError(val.DisplayValue);
        } else if (val.Flags.HasFlag(MonoClient.ObjectValueFlags.Unknown)) {
            response.SetError("invalid expression");
        } else if (val.Flags.HasFlag(MonoClient.ObjectValueFlags.Object) && val.Flags.HasFlag(MonoClient.ObjectValueFlags.Namespace)) {
            response.SetError("not available");
        } else {
            int handle = 0;
            if (val.HasChildren) {
                handle = this.variableHandles.Create(val.GetAllChildren());
            }
            response.SetSuccess(new DebugProtocol.EvaluateResponseBody(val.DisplayValue, handle));
        }
    }
#endregion
#region request: Source
    protected override void Source(DebugProtocol.Response response, DebugProtocol.Arguments arguments) {
        response.SetError("No source available");
    }
#endregion
#region request: ExceptionInfo
    protected override void ExceptionInfo(DebugProtocol.Response response, DebugProtocol.Arguments arguments) {
        var ex = DebuggerActiveException(arguments.ThreadId);
        if (ex == null) {
            response.SetError("No exception available");
            return;
        }

        response.SetSuccess(new DebugProtocol.ExceptionInfoResponseBody(ex));
    }
#endregion

#region Event handlers 

    private void TargetStopped(object sender, MonoClient.TargetEventArgs e) {
        Stopped();
        SendMessage(new DebugProtocol.Events.StoppedEvent((int)e.Thread.Id, "pause"));
        this.resumeEvent.Set();
    }
    private void TargetHitBreakpoint(object sender, MonoClient.TargetEventArgs e) {
        Stopped();
        SendMessage(new DebugProtocol.Events.StoppedEvent((int)e.Thread.Id, "breakpoint"));
        this.resumeEvent.Set();
    }
    private void TargetExceptionThrown(object sender, MonoClient.TargetEventArgs e) {
        Stopped();
        var ex = DebuggerActiveException((int)e.Thread.Id);
        if (ex != null)
            SendMessage(new DebugProtocol.Events.StoppedEvent((int)e.Thread.Id, "exception", ex.Message));
        this.resumeEvent.Set();
    }
    private void TargetReady(object sender, MonoClient.TargetEventArgs e) {
        this.activeProcess = this.session.GetProcesses().SingleOrDefault();
        SendMessage(new DebugProtocol.Events.InitializedEvent());
    }
    private void TargetExited(object sender, MonoClient.TargetEventArgs e) {
        KillDebugger();
        this.resumeEvent.Set();
    }
    private void TargetInterrupted(object sender, MonoClient.TargetEventArgs e) {
        this.resumeEvent.Set();
    }
    private void TargetThreadStarted(object sender, MonoClient.TargetEventArgs e) {
        int tid = (int)e.Thread.Id;
        lock (this.seenThreads) {
            this.seenThreads[tid] = new DebugProtocol.Types.Thread(tid, e.Thread.Name);
        }
        SendMessage(new DebugProtocol.Events.ThreadEvent("started", tid));
    }
    private void TargetThreadStopped(object sender, MonoClient.TargetEventArgs e) {
        int tid = (int)e.Thread.Id;
        lock (this.seenThreads) {
            this.seenThreads.Remove(tid);
        }
        SendMessage(new DebugProtocol.Events.ThreadEvent("exited", tid));
    }

    private bool OnExceptionHandled(Exception ex) {
        this.sessionLogger.Error(ex);
        var innerException = ex.InnerException;

        while (innerException != null) {
            this.sessionLogger.Error(innerException);
            innerException = innerException.InnerException;
        }
        return true;
    }

    private void OnSessionLog(bool isError, string message) {
        if (isError) this.sessionLogger.Error($"Mono: {message.Trim()}");
        else this.sessionLogger.Debug($"Mono: {message.Trim()}");
    }
    private void OnLog(bool isError, string message) {
        if (isError) OnErrorDataReceived(message);
        else OnOutputDataReceived(message);
    }
    private void OnDebugLog(int level, string category, string message) {
        SendConsoleEvent("console", message);
    }

#endregion
#region Helpers
    private void KillDebugger() {
        lock (this.locker) {
            foreach(var process in this.processes)
                process.Kill();

            this.processes.Clear();

            if (this.session != null) {
                this.debuggerExecuting = true;

                if (!this.session.HasExited)
                    this.session.Exit();

                this.session.Dispose();
                this.session = null;
            }

            if (!this.terminated) {
                SendMessage(new DebugProtocol.Events.TerminatedEvent());
                this.terminated = true;
            }
        }
    }

    private void WaitForSuspend() {
        if (this.debuggerExecuting) {
            this.resumeEvent.WaitOne();
            this.debuggerExecuting = false;
        }
    }

    private MonoClient.ThreadInfo FindThread(int threadReference) {
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

    private DebugProtocol.Types.Variable CreateVariable(MonoClient.ObjectValue v) {
        var dv = v.DisplayValue ?? "<error getting value>";
        int childrenReference = 0;

        if (dv.Length > 1 && dv[0] == '{' && dv[dv.Length - 1] == '}')
            dv = dv.Substring(1, dv.Length - 2);

        if (v.HasChildren) {
            var objectValues = v.GetAllChildren();
            childrenReference = this.variableHandles.Create(objectValues);
        }

        return new DebugProtocol.Types.Variable(v.Name, dv, v.TypeName, childrenReference);
    }

    private MonoClient.ThreadInfo DebuggerActiveThread() {
        lock (this.locker) {
            return this.session.ActiveThread;
        }
    }

    private MonoClient.ExceptionInfo DebuggerActiveException(int threadId) {
        var thread = FindThread(threadId);
        if (thread == null)
            return null;

        for (int i = 0; i < thread.Backtrace.FrameCount; i++) {
            var ex = thread.Backtrace.GetFrame(i).GetException();
            if (ex != null) {
                this.exception = ex.Instance;
                return ex;
            }
        }
        return null;
    }

#endregion
}
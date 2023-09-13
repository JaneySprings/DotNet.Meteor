using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using Mono.Debugging.Soft;
using DotNet.Meteor.Debug.Utilities;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using MonoClient = Mono.Debugging.Client;
using DebugProtocol = Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;

namespace DotNet.Meteor.Debug;

public partial class DebugSession : Session {
    private MonoClient.ObjectValue exception;
    private MonoClient.ProcessInfo activeProcess;
    private SymbolServer symbolServer;

    private readonly List<Action> disposables = new List<Action>();
    private readonly AutoResetEvent suspendEvent = new AutoResetEvent(false);
    private readonly Handles<MonoClient.StackFrame> frameHandles = new Handles<MonoClient.StackFrame>();
    private readonly Handles<MonoClient.ObjectValue[]> variableHandles = new Handles<MonoClient.ObjectValue[]>();
    private readonly Dictionary<int, DebugProtocol.Thread> seenThreads = new Dictionary<int, DebugProtocol.Thread>();
    private readonly SoftDebuggerSession session = new SoftDebuggerSession {
        Breakpoints = new MonoClient.BreakpointStore()
    };
    private readonly MonoClient.DebuggerSessionOptions sessionOptions = new MonoClient.DebuggerSessionOptions {
        EvaluationOptions = new MonoClient.EvaluationOptions {
            EvaluationTimeout = 5000,
            MemberEvaluationTimeout = 5000,
            UseExternalTypeResolver = false,
            AllowMethodEvaluation = true,
            GroupPrivateMembers = true,
            GroupStaticMembers = true,
            AllowToStringCalls = true,
            AllowTargetInvoke = true,
            ChunkRawStrings = false,
            EllipsizeStrings = false,
            CurrentExceptionTag = "$exception",
            IntegerDisplayFormat = MonoClient.IntegerDisplayFormat.Decimal,
            StackFrameFormat = new MonoClient.StackFrameFormat()
        }
    };

    public DebugSession(Stream input, Stream output): base(input, output) {
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

    protected override MonoClient.ICustomLogger GetLogger() => MonoClient.DebuggerLoggingService.CustomLogger;

#region request: Initialize
    protected override InitializeResponse HandleInitializeRequest(InitializeArguments arguments) {
        return new InitializeResponse() {
            SupportsEvaluateForHovers = true,
            SupportsExceptionInfoRequest = true,
            SupportsConditionalBreakpoints = true,
            SupportsHitConditionalBreakpoints = true,
            SupportsLogPoints = true,
            SupportsExceptionOptions = true,
            SupportsExceptionFilterOptions = true,
            ExceptionBreakpointFilters = new List<ExceptionBreakpointsFilter> {
                ExceptionsFilter.AllExceptions
            }
        };
    }
#endregion request: Initialize
#region request: Launch
    protected override LaunchResponse HandleLaunchRequest(LaunchArguments arguments) {
        var configuration = new LaunchConfiguration(arguments.ConfigurationProperties);
        symbolServer = new SymbolServer(configuration.Project.Path);

        if (configuration.DebugPort == 0)
            configuration.DebugPort = Extensions.FindFreePort();
        if (configuration.DebugPort < 1)
            throw new ProtocolException($"Invalid port '{configuration.DebugPort}'");

        LaunchApplication(configuration, configuration.DebugPort);
        Connect(configuration, configuration.DebugPort);
        return new LaunchResponse();
    }
#endregion request: Launch
#region request: Disconnect
    protected override DisconnectResponse HandleDisconnectRequest(DisconnectArguments arguments) {
        if (this.session?.IsRunning == true)
            this.session.Stop();

        foreach(var disposable in disposables)
            disposable.Invoke();

        this.disposables.Clear();
        if (this.session != null) {
            if (!this.session.HasExited)
                this.session.Exit();

            this.session.Dispose();
        }

        return new DisconnectResponse();
    }
#endregion request: Disconnect
#region request: Continue
    protected override ContinueResponse HandleContinueRequest(ContinueArguments arguments) {
        return DoSafe<ContinueResponse>(() => {
            if (this.session?.IsRunning == false && !this.session.HasExited)
                this.session.Continue();

            return new ContinueResponse();
        });
    }
#endregion request: Continue
#region request: Next
    protected override NextResponse HandleNextRequest(NextArguments arguments) {
        return DoSafe<NextResponse>(() => {
            if (this.session?.IsRunning == false && !this.session.HasExited)
                this.session.NextLine();

            return new NextResponse();
        });
    }
#endregion request: Next
#region request: StepIn
    protected override StepInResponse HandleStepInRequest(StepInArguments arguments) {
        return DoSafe<StepInResponse>(() => {
            if (this.session?.IsRunning == false && !this.session.HasExited)
                this.session.StepLine();

            return new StepInResponse();
        });
    }
#endregion request: StepIn
#region request: StepOut
    protected override StepOutResponse HandleStepOutRequest(StepOutArguments arguments) {
        return DoSafe<StepOutResponse>(() => {
            if (this.session?.IsRunning == false && !this.session.HasExited)
                this.session.Finish();

            return new StepOutResponse();
        });
    }
#endregion request: StepOut
#region request: Pause
    protected override PauseResponse HandlePauseRequest(PauseArguments arguments) {
        return DoSafe<PauseResponse>(() => {
            if (this.session?.IsRunning == true)
                this.session.Stop();

            return new PauseResponse();
        });
    }
#endregion request: Pause
#region request: SetExceptionBreakpoints
    protected override SetExceptionBreakpointsResponse HandleSetExceptionBreakpointsRequest(SetExceptionBreakpointsArguments arguments) {
        this.session.Breakpoints.ClearCatchpoints();
        if (arguments.FilterOptions == null || arguments.FilterOptions.Count == 0)
            return new SetExceptionBreakpointsResponse();

        foreach (var option in arguments.FilterOptions) {
            if (option.FilterId == ExceptionsFilter.AllExceptions.Filter) {
                var exceptionFilter = typeof(Exception).ToString();

                if (!string.IsNullOrEmpty(option.Condition))
                    exceptionFilter = option.Condition;

                foreach(var exception in exceptionFilter.Split(','))
                    this.session.Breakpoints.AddCatchpoint(exception);
            }
        }
        return new SetExceptionBreakpointsResponse();
    }
#endregion request: SetExceptionBreakpoints
#region request: SetBreakpoints
    protected override SetBreakpointsResponse HandleSetBreakpointsRequest(SetBreakpointsArguments arguments) {
        var breakpoints = new List<DebugProtocol.Breakpoint>();
        var breakpointsInfos = arguments.Breakpoints;
        var sourcePath = arguments.Source?.Path;

        // Remove all file breakpoints
        var fileBreakpoints = this.session.Breakpoints.GetBreakpointsAtFile(sourcePath);
        foreach(var fileBreakpoint in fileBreakpoints) {
            this.session.Breakpoints.Remove(fileBreakpoint);
        }
        // Add all new breakpoints
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
                breakpoint.TraceExpression = $"[LogPoint]: {breakpointInfo.LogMessage}";
            }

            breakpoints.Add(new DebugProtocol.Breakpoint() {
                Verified = breakpoint != null,
                Line =  breakpoint?.Line ?? breakpointInfo.Line,
                Column = breakpoint?.Column ?? breakpointInfo.Column
            });
        }

        return new SetBreakpointsResponse(breakpoints);
    }
#endregion request: SetBreakpoints
#region request: StackTrace
    protected override StackTraceResponse HandleStackTraceRequest(StackTraceArguments arguments) {
        return DoSafe<StackTraceResponse>(() => {
            MonoClient.ThreadInfo thread = this.session.ActiveThread;
            if (thread.Id != arguments.ThreadId) {
                thread = FindThread(arguments.ThreadId);
                thread?.SetActive();
            }

            var stackFrames = new List<DebugProtocol.StackFrame>();
            var bt = thread.Backtrace;

            if (bt?.FrameCount < 0)
                throw new ProtocolException("No stack trace available");

            int totalFrames = bt.FrameCount;
            int startFrame = arguments.StartFrame ?? 0;
            int levels = arguments.Levels ?? totalFrames;
            for (int i = startFrame; i < Math.Min(startFrame + levels, totalFrames); i++) {
                DebugProtocol.Source source = null;
                var hint = DebugProtocol.StackFrame.PresentationHintValue.Unknown;
                var frame = bt.GetFrame(i);
                var sourceLocation = frame.SourceLocation;
                string sourceName = string.Empty;

                if (!string.IsNullOrEmpty(sourceLocation.FileName)) {
                    sourceName = Path.GetFileName(sourceLocation.FileName);
                    if (File.Exists(sourceLocation.FileName)) {
                        var path = sourceLocation.FileName;
                        hint = DebugProtocol.StackFrame.PresentationHintValue.Normal;
                        source = new DebugProtocol.Source() {
                            PresentationHint = DebugProtocol.Source.PresentationHintValue.Normal,
                            SourceReference = 0,
                            Name = sourceName,
                            Path = path
                        };
                    }
                }
                if (sourceLocation.SourceLink != null && source == null) {
                    sourceName = Path.GetFileName(sourceLocation.SourceLink.RelativeFilePath);
                    string path = symbolServer.DownloadSourceFile(sourceLocation.SourceLink.Uri, sourceLocation.SourceLink.RelativeFilePath);
                    if (!string.IsNullOrEmpty(path)) {
                        hint = DebugProtocol.StackFrame.PresentationHintValue.Normal;
                        source = new DebugProtocol.Source() {
                            PresentationHint = DebugProtocol.Source.PresentationHintValue.Normal,
                            SourceReference = 0,
                            Name = sourceName,
                            Path = path
                        };
                    }
                }
                if (source == null) {
                    hint = DebugProtocol.StackFrame.PresentationHintValue.Subtle;
                    source = new DebugProtocol.Source() {
                        PresentationHint = DebugProtocol.Source.PresentationHintValue.Deemphasize,
                        SourceReference = 1000,
                        Name = sourceName,
                    };
                }

                stackFrames.Add(new DebugProtocol.StackFrame() {
                    Id = this.frameHandles.Create(frame),
                    Source = source,
                    PresentationHint = hint,
                    Name = frame.SourceLocation.MethodName,
                    Line = frame.SourceLocation.Line,
                    Column = frame.SourceLocation.Column,
                    EndLine = frame.SourceLocation.EndLine,
                    EndColumn = frame.SourceLocation.EndColumn
                });
            }

            return new StackTraceResponse(stackFrames);
        });
    }
#endregion request: StackTrace
#region request: Scopes
    protected override ScopesResponse HandleScopesRequest(ScopesArguments arguments) {
        return DoSafe<ScopesResponse>(() => {
            int frameId = arguments.FrameId;
            var frame = this.frameHandles.Get(frameId, null);
            var scopes = new List<DebugProtocol.Scope>();

            if (frame == null)
                throw new ProtocolException("frame not found");

            if (this.exception != null) {
                scopes.Add(new DebugProtocol.Scope() {
                    Name = "Exception",
                    VariablesReference = this.variableHandles.Create(new MonoClient.ObjectValue[] { this.exception })
                });
            }

            scopes.Add(new DebugProtocol.Scope() {
                Name = "Local",
                VariablesReference = this.variableHandles.Create(frame.GetAllLocals())
            });

            return new ScopesResponse(scopes);
        });
    }
#endregion request: Scopes
#region request: Variables
    protected override VariablesResponse HandleVariablesRequest(VariablesArguments arguments) {
        return DoSafe<VariablesResponse>(() => {
            int reference = arguments.VariablesReference;
            if (reference == -1)
                throw new ProtocolException("variables: property 'variablesReference' is missing");

            var variables = new List<DebugProtocol.Variable>();

            if (this.variableHandles.TryGet(reference, out MonoClient.ObjectValue[] children) && children?.Length > 0) {
                if (children.Length < 20) {
                    // Wait for all values at once.
                    WaitHandle.WaitAll(children.Select(x => x.WaitHandle).ToArray());
                    foreach (var v in children) {
                        variables.Add(CreateVariable(v));
                    }
                } else {
                    foreach (var v in children) {
                        v.WaitHandle.WaitOne(this.session.EvaluationOptions.EvaluationTimeout);
                        variables.Add(CreateVariable(v));
                    }
                }
            }

            return new VariablesResponse(variables);
        });
    }
#endregion request: Variables
#region request: Threads
    protected override ThreadsResponse HandleThreadsRequest(ThreadsArguments arguments) {
        return DoSafe<ThreadsResponse>(() => {
            var threads = new List<DebugProtocol.Thread>();
            var process = this.activeProcess;
            if (process != null) {
                Dictionary<int, DebugProtocol.Thread> d;
                lock (this.seenThreads) {
                    d = new Dictionary<int, DebugProtocol.Thread>(this.seenThreads);
                }
                foreach (var t in process.GetThreads()) {
                    int tid = (int)t.Id;
                    d[tid] = new DebugProtocol.Thread(tid, t.Name.ToThreadName(tid));
                }
                threads = d.Values.ToList();
            }
            return new ThreadsResponse(threads);
        });
    }
#endregion request: Threads
#region request: Evaluate
    protected override EvaluateResponse HandleEvaluateRequest(EvaluateArguments arguments) {
        return DoSafe<EvaluateResponse>(() => {
            if (string.IsNullOrEmpty(arguments.Expression))
                throw new ProtocolException("expression missing");

            var frame = this.frameHandles.Get(arguments.FrameId ?? 0, null);
            if (frame == null)
                throw new ProtocolException("no active stackframe");

            if (!frame.ValidateExpression(arguments.Expression))
                throw new ProtocolException("invalid expression");

            var val = frame.GetExpressionValue(arguments.Expression, this.sessionOptions.EvaluationOptions);
            val.WaitHandle.WaitOne(this.sessionOptions.EvaluationOptions.EvaluationTimeout);

            if (val.IsEvaluating)
                throw new ProtocolException("evaluation timeout expected");
            if (val.Flags.HasFlag(MonoClient.ObjectValueFlags.Error) || val.Flags.HasFlag(MonoClient.ObjectValueFlags.NotSupported))
                throw new ProtocolException(val.DisplayValue);
            if (val.Flags.HasFlag(MonoClient.ObjectValueFlags.Unknown))
                throw new ProtocolException("invalid expression");
            if (val.Flags.HasFlag(MonoClient.ObjectValueFlags.Object) && val.Flags.HasFlag(MonoClient.ObjectValueFlags.Namespace))
                throw new ProtocolException("not available");

            int handle = 0;
            if (val.HasChildren)
                handle = this.variableHandles.Create(val.GetAllChildren());

            return new EvaluateResponse(val.DisplayValue, handle);
        });
    }
#endregion request: Evaluate
#region request: Source
    protected override SourceResponse HandleSourceRequest(SourceArguments arguments) {
        throw new ProtocolException("No source available");
    }
#endregion request: Source
#region request: ExceptionInfo
    protected override ExceptionInfoResponse HandleExceptionInfoRequest(ExceptionInfoArguments arguments) {
        return DoSafe<ExceptionInfoResponse>(() => {
            var ex = GetActiveException(arguments.ThreadId);
            if (ex == null)
                throw new ProtocolException("No exception available");

            return new ExceptionInfoResponse(ex.Type, DebugProtocol.ExceptionBreakMode.Always) {
                Description = ex.Message
            };
        });
    }
#endregion request: ExceptionInfo

#region Event handlers 

    private void TargetStopped(object sender, MonoClient.TargetEventArgs e) {
        Reset();
        this.suspendEvent.Set();
        Protocol.SendEvent(new StoppedEvent(StoppedEvent.ReasonValue.Pause) {
            ThreadId = (int)e.Thread.Id,
            AllThreadsStopped = true,
        });
    }
    private void TargetHitBreakpoint(object sender, MonoClient.TargetEventArgs e) {
        Reset();
        this.suspendEvent.Set();
        Protocol.SendEvent(new StoppedEvent(StoppedEvent.ReasonValue.Breakpoint) {
            ThreadId = (int)e.Thread.Id,
            AllThreadsStopped = true,
        });
    }
    private void TargetExceptionThrown(object sender, MonoClient.TargetEventArgs e) {
        Reset();
        this.suspendEvent.Set();
        var ex = GetActiveException((int)e.Thread.Id);
        if (ex != null) {
            Protocol.SendEvent(new StoppedEvent(StoppedEvent.ReasonValue.Exception) {
                ThreadId = (int)e.Thread.Id,
                AllThreadsStopped = true,
                Text = ex.Message
            });
        }
    }
    private void TargetReady(object sender, MonoClient.TargetEventArgs e) {
        this.activeProcess = this.session.GetProcesses().SingleOrDefault();
        Protocol.SendEvent(new InitializedEvent());
    }
    private void TargetExited(object sender, MonoClient.TargetEventArgs e) {
        Protocol.SendEvent(new TerminatedEvent());
    }
    private void TargetInterrupted(object sender, MonoClient.TargetEventArgs e) {
        this.suspendEvent.Set();
    }
    private void TargetThreadStarted(object sender, MonoClient.TargetEventArgs e) {
        int tid = (int)e.Thread.Id;
        lock (this.seenThreads) {
            this.seenThreads[tid] = new DebugProtocol.Thread(tid, e.Thread.Name.ToThreadName(tid));
        }
        Protocol.SendEvent(new ThreadEvent(ThreadEvent.ReasonValue.Started, tid));
    }
    private void TargetThreadStopped(object sender, MonoClient.TargetEventArgs e) {
        int tid = (int)e.Thread.Id;
        lock (this.seenThreads) {
            this.seenThreads.Remove(tid);
        }
        Protocol.SendEvent(new ThreadEvent(ThreadEvent.ReasonValue.Exited, tid));
    }

    private bool OnExceptionHandled(Exception ex) {
        GetLogger().LogError($"[Handled] {ex.Message}", ex);
        return true;
    }

    private void OnSessionLog(bool isError, string message) {
        if (isError) GetLogger().LogError($"[Error] {message.Trim()}", null);
        else GetLogger().LogMessage($"[Info] {message.Trim()}");

        SendConsoleEvent(OutputEvent.CategoryValue.Stdout, $"[Mono] {message.Trim()}");
    }
    private void OnLog(bool isError, string message) {
        if (isError) OnErrorDataReceived(message);
        else OnOutputDataReceived(message);
    }
    private void OnDebugLog(int level, string category, string message) {
        SendConsoleEvent(OutputEvent.CategoryValue.Console, message);
    }

#endregion Event handlers 
#region Helpers
    private void Reset() {
        this.exception = null;
        this.variableHandles.Reset();
        this.frameHandles.Reset();
    }

    private MonoClient.ThreadInfo FindThread(int threadReference) {
        if (this.activeProcess != null) {
            foreach (var t in this.activeProcess.GetThreads()) {
                if (t.Id == threadReference)
                    return t;
            }
        }

        return null;
    }
    private DebugProtocol.Variable CreateVariable(MonoClient.ObjectValue v) {
        var dv = v.DisplayValue ?? "<error getting value>";
        int childrenReference = 0;

        if (dv.Length > 1 && dv[0] == '{' && dv[dv.Length - 1] == '}')
            dv = dv.Substring(1, dv.Length - 2);

        if (v.HasChildren) {
            var objectValues = v.GetAllChildren();
            childrenReference = this.variableHandles.Create(objectValues);
        }

        return new DebugProtocol.Variable(v.Name, dv, childrenReference) {
            VariablesReference = childrenReference
        };
    }

    private MonoClient.ExceptionInfo GetActiveException(int threadId) {
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
#endregion Helpers
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using Mono.Debugging.Soft;
using DotNet.Meteor.Debug.Extensions;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using MonoClient = Mono.Debugging.Client;
using DebugProtocol = Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;

namespace DotNet.Meteor.Debug;

public class DebugSession : Session {
    private ExternalTypeResolver typeResolver;
    private SymbolServer symbolServer;
    private BaseLaunchAgent launchAgent;

    private readonly Handles<MonoClient.StackFrame> frameHandles = new Handles<MonoClient.StackFrame>();
    private readonly Handles<MonoClient.ObjectValue[]> variableHandles = new Handles<MonoClient.ObjectValue[]>();
    private readonly SoftDebuggerSession session = new SoftDebuggerSession();

    public DebugSession(Stream input, Stream output): base(input, output) {
        MonoClient.DebuggerLoggingService.CustomLogger = new MonoLogger();

        session.LogWriter = OnSessionLog;
        session.DebugWriter = OnDebugLog;
        session.OutputWriter = OnLog;
        session.ExceptionHandler = OnExceptionHandled;

        session.TargetStopped += TargetStopped;
        session.TargetHitBreakpoint += TargetHitBreakpoint;
        session.TargetExceptionThrown += TargetExceptionThrown;
        session.TargetUnhandledException += TargetExceptionThrown;
        session.TargetReady += TargetReady;
        session.TargetExited += TargetExited;
        session.TargetThreadStarted += TargetThreadStarted;
        session.TargetThreadStopped += TargetThreadStopped;
    }

    protected override MonoClient.ICustomLogger GetLogger() => MonoClient.DebuggerLoggingService.CustomLogger;
    protected override void OnUnhandledException(Exception ex) => launchAgent?.Dispose();

#region request: Initialize
    protected override InitializeResponse HandleInitializeRequest(InitializeArguments arguments) {
        return new InitializeResponse() {
            SupportsTerminateRequest = true,
            SupportsEvaluateForHovers = true,
            SupportsExceptionInfoRequest = true,
            SupportsConditionalBreakpoints = true,
            SupportsHitConditionalBreakpoints = true,
            SupportsLogPoints = true,
            SupportsExceptionOptions = true,
            SupportsExceptionFilterOptions = true,
            SupportsCompletionsRequest = true,
            CompletionTriggerCharacters = new List<string> { "." },
            ExceptionBreakpointFilters = new List<ExceptionBreakpointsFilter> {
                ExceptionsFilter.AllExceptions
            }
        };
    }
#endregion request: Initialize
#region request: Launch
    protected override LaunchResponse HandleLaunchRequest(LaunchArguments arguments) {
        return DoSafe<LaunchResponse>(() => {
            var configuration = new LaunchConfiguration(arguments.ConfigurationProperties);
            launchAgent = configuration.GetLauchAgent();

            symbolServer = new SymbolServer(configuration.TempDirectoryPath);
            typeResolver = new ExternalTypeResolver(configuration.TempDirectoryPath, configuration.DebuggerSessionOptions);
            launchAgent.Disposables.Add(() => symbolServer.Dispose());
            launchAgent.Disposables.Add(() => typeResolver.Dispose());
            session.TypeResolverHandler = typeResolver.Handle;

            launchAgent.Launch(this);
            launchAgent.Connect(session);
            return new LaunchResponse();
        });
    }
#endregion request: Launch
#region request: Terminate
    protected override TerminateResponse HandleTerminateRequest(TerminateArguments arguments) {
        if (!session.HasExited)
            session.Exit();

        launchAgent?.Dispose();
        if (launchAgent is not DebugLaunchAgent)
            Protocol.SendEvent(new TerminatedEvent());

        return new TerminateResponse();
    }
#endregion request: Terminate
#region request: Disconnect
    protected override DisconnectResponse HandleDisconnectRequest(DisconnectArguments arguments) {
        session.Dispose();
        return new DisconnectResponse();
    }
#endregion request: Disconnect
#region request: Continue
    protected override ContinueResponse HandleContinueRequest(ContinueArguments arguments) {
        return DoSafe<ContinueResponse>(() => {
            if (!session.IsRunning && !session.HasExited)
                session.Continue();

            return new ContinueResponse();
        });
    }
#endregion request: Continue
#region request: Next
    protected override NextResponse HandleNextRequest(NextArguments arguments) {
        return DoSafe<NextResponse>(() => {
            if (!session.IsRunning && !session.HasExited)
                session.NextLine();

            return new NextResponse();
        });
    }
#endregion request: Next
#region request: StepIn
    protected override StepInResponse HandleStepInRequest(StepInArguments arguments) {
        return DoSafe<StepInResponse>(() => {
            if (!session.IsRunning && !session.HasExited)
                session.StepLine();

            return new StepInResponse();
        });
    }
#endregion request: StepIn
#region request: StepOut
    protected override StepOutResponse HandleStepOutRequest(StepOutArguments arguments) {
        return DoSafe<StepOutResponse>(() => {
            if (!session.IsRunning && !session.HasExited)
                session.Finish();

            return new StepOutResponse();
        });
    }
#endregion request: StepOut
#region request: Pause
    protected override PauseResponse HandlePauseRequest(PauseArguments arguments) {
        return DoSafe<PauseResponse>(() => {
            if (session.IsRunning)
                session.Stop();

            return new PauseResponse();
        });
    }
#endregion request: Pause
#region request: SetExceptionBreakpoints
    protected override SetExceptionBreakpointsResponse HandleSetExceptionBreakpointsRequest(SetExceptionBreakpointsArguments arguments) {
        session.Breakpoints.ClearCatchpoints();
        if (arguments.FilterOptions == null || arguments.FilterOptions.Count == 0)
            return new SetExceptionBreakpointsResponse();

        foreach (var option in arguments.FilterOptions) {
            if (option.FilterId == ExceptionsFilter.AllExceptions.Filter) {
                var exceptionFilter = typeof(Exception).ToString();

                if (!string.IsNullOrEmpty(option.Condition))
                    exceptionFilter = option.Condition;

                foreach(var exception in exceptionFilter.Split(','))
                    session.Breakpoints.AddCatchpoint(exception);
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
        var fileBreakpoints = session.Breakpoints.GetBreakpointsAtFile(sourcePath);
        foreach(var fileBreakpoint in fileBreakpoints)
            session.Breakpoints.Remove(fileBreakpoint);

        // Add all new breakpoints
        foreach(var breakpointInfo in breakpointsInfos) {
            MonoClient.Breakpoint breakpoint = session.Breakpoints.Add(sourcePath, breakpointInfo.Line, breakpointInfo.Column ?? 1);
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
            var thread = session.ActiveThread;
            if (thread.Id != arguments.ThreadId) {
                thread = session.FindThread(arguments.ThreadId);
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
                var frame = bt.GetFrameSafe(i);
                if (frame == null) {
                    stackFrames.Add(new DebugProtocol.StackFrame(0, "<unknown>", 0, 0));
                    continue;
                }

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
                    Id = frameHandles.Create(frame),
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
            var frame = frameHandles.Get(frameId, null);
            var scopes = new List<DebugProtocol.Scope>();

            if (frame == null)
                throw new ProtocolException("frame not found");

            scopes.Add(new DebugProtocol.Scope() {
                Name = "Locals",
                VariablesReference = variableHandles.Create(frame.GetAllLocals())
            });

            return new ScopesResponse(scopes);
        });
    }
#endregion request: Scopes
#region request: Variables
    protected override VariablesResponse HandleVariablesRequest(VariablesArguments arguments) {
        return DoSafe<VariablesResponse>(() => {
            var reference = arguments.VariablesReference;
            var variables = new List<DebugProtocol.Variable>();
            if (variableHandles.TryGet(reference, out MonoClient.ObjectValue[] children) && children?.Length > 0) {
                if (children.Length < 20) {
                    // Wait for all values at once.
                    WaitHandle.WaitAll(children.Select(x => x.WaitHandle).ToArray());
                    foreach (var v in children) {
                        variables.Add(CreateVariable(v));
                    }
                } else {
                    foreach (var v in children) {
                        v.WaitHandle.WaitOne(session.EvaluationOptions.EvaluationTimeout);
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
            var threads = new Dictionary<int, DebugProtocol.Thread>();
            var process = session.GetProcesses().FirstOrDefault();
            if (process == null)
                return new ThreadsResponse();

            foreach (var thread in process.GetThreads()) {
                int tid = (int)thread.Id;
                threads[tid] = new DebugProtocol.Thread(tid, thread.Name.ToThreadName(tid));
            }
         
            return new ThreadsResponse(threads.Values.ToList());
        });
    }
#endregion request: Threads
#region request: Evaluate
    protected override EvaluateResponse HandleEvaluateRequest(EvaluateArguments arguments) {
        return DoSafe<EvaluateResponse>(() => {
            if (arguments.Expression.StartsWith(BaseLaunchAgent.CommandPrefix)) {
                launchAgent?.HandleCommand(arguments.Expression, this);
                throw new ProtocolException($"command handled by {launchAgent}");
            }

            var frame = frameHandles.Get(arguments.FrameId ?? 0, null);
            if (frame == null)
                throw new ProtocolException("no active stackframe");

            if (!frame.ValidateExpression(arguments.Expression))
                throw new ProtocolException("invalid expression");

            var value = frame.GetExpressionValue(arguments.Expression, session.Options.EvaluationOptions);
            value.WaitHandle.WaitOne(session.Options.EvaluationOptions.EvaluationTimeout);

            if (value.IsEvaluating)
                throw new ProtocolException("evaluation timeout expected");
            if (value.Flags.HasFlag(MonoClient.ObjectValueFlags.Error) || value.Flags.HasFlag(MonoClient.ObjectValueFlags.NotSupported))
                throw new ProtocolException(value.DisplayValue);
            if (value.Flags.HasFlag(MonoClient.ObjectValueFlags.Unknown))
                throw new ProtocolException("invalid expression");
            if (value.Flags.HasFlag(MonoClient.ObjectValueFlags.Object) && value.Flags.HasFlag(MonoClient.ObjectValueFlags.Namespace))
                throw new ProtocolException("not available");

            int handle = 0;
            if (value.HasChildren)
                handle = variableHandles.Create(value.GetAllChildren());

            return new EvaluateResponse(value.ToDisplayValue(), handle);
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
            var ex = session.FindException(arguments.ThreadId);
            if (ex == null)
                throw new ProtocolException("No exception available");

            return new ExceptionInfoResponse(ex.Type, DebugProtocol.ExceptionBreakMode.Always) {
                Description = ex.Message
            };
        });
    }
#endregion request: ExceptionInfo
#region request: Completions
    protected override CompletionsResponse HandleCompletionsRequest(CompletionsArguments arguments) {
        return DoSafe<CompletionsResponse>(() => {
            if (arguments.Text.StartsWith(BaseLaunchAgent.CommandPrefix))
                return new CompletionsResponse(launchAgent?.GetCompletionItems());

            var frame = frameHandles.Get(arguments.FrameId ?? 0, null);
            if (frame == null)
                throw new ProtocolException("no active stackframe");

            string resolvedText = null;
            if (session.Options.EvaluationOptions.UseExternalTypeResolver) {
                var lastTriggerIndex = arguments.Text.LastIndexOf('.');
                if (lastTriggerIndex > 0) {
                    resolvedText = frame.ResolveExpression(arguments.Text.Substring(0, lastTriggerIndex));
                    resolvedText += arguments.Text.Substring(lastTriggerIndex);
                }
            }

            var completionData = frame.GetExpressionCompletionData(resolvedText ?? arguments.Text);
            if (completionData == null || completionData.Items == null)
                return new CompletionsResponse();

            return new CompletionsResponse(completionData.Items.Select(x => x.ToCompletionItem()).ToList());
        });
    }
#endregion request: Completions

    private void TargetStopped(object sender, MonoClient.TargetEventArgs e) {
        ResetHandles();
        Protocol.SendEvent(new StoppedEvent(StoppedEvent.ReasonValue.Pause) {
            ThreadId = (int)e.Thread.Id,
            AllThreadsStopped = true,
        });
    }
    private void TargetHitBreakpoint(object sender, MonoClient.TargetEventArgs e) {
        ResetHandles();
        Protocol.SendEvent(new StoppedEvent(StoppedEvent.ReasonValue.Breakpoint) {
            ThreadId = (int)e.Thread.Id,
            AllThreadsStopped = true,
        });
    }
    private void TargetExceptionThrown(object sender, MonoClient.TargetEventArgs e) {
        ResetHandles();
        var ex = session.FindException(e.Thread.Id);
        Protocol.SendEvent(new StoppedEvent(StoppedEvent.ReasonValue.Exception) {
            Description = "Paused on exception",
            Text = ex.Type ?? "Exception",
            ThreadId = (int)e.Thread.Id,
            AllThreadsStopped = true,
        });
    }
    private void TargetReady(object sender, MonoClient.TargetEventArgs e) {
        Protocol.SendEvent(new InitializedEvent());
    }
    private void TargetExited(object sender, MonoClient.TargetEventArgs e) {
        Protocol.SendEvent(new TerminatedEvent());
    }
    private void TargetThreadStarted(object sender, MonoClient.TargetEventArgs e) {
        int tid = (int)e.Thread.Id;
        Protocol.SendEvent(new ThreadEvent(ThreadEvent.ReasonValue.Started, tid));
    }
    private void TargetThreadStopped(object sender, MonoClient.TargetEventArgs e) {
        int tid = (int)e.Thread.Id;
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

    private void ResetHandles() {
        variableHandles.Reset();
        frameHandles.Reset();
    }
    private DebugProtocol.Variable CreateVariable(MonoClient.ObjectValue v) {
        var dv = v.ToDisplayValue();
        var childrenReference = 0;
        if (v.HasChildren) {
            var objectValues = v.GetAllChildren();
            childrenReference = variableHandles.Create(objectValues);
        }
        return new DebugProtocol.Variable(v.Name, dv, childrenReference) {
            VariablesReference = childrenReference
        };
    }
}
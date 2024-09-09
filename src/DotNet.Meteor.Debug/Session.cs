using DotNet.Meteor.Processes;
using Mono.Debugging.Client;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using DotNet.Meteor.Common;
using DotNet.Meteor.Debug.Extensions;

namespace DotNet.Meteor.Debug;

public abstract class Session : DebugAdapterBase, IProcessLogger {

    protected Session(Stream input, Stream output) {
        LogConfig.InitializeLog();
        InitializeProtocolClient(input, output);
    }

    protected abstract void OnUnhandledException(Exception ex);
    protected abstract void HandleHotReloadRequest(IRequestResponder<HotReloadArguments> responder);

    public void Start() {
        Protocol.LogMessage += LogMessage;
        Protocol.DispatcherError += LogError;
        Protocol.RegisterRequestType<HotReloadRequest, HotReloadArguments>(HandleHotReloadRequest);
        Protocol.Run();
    }
    public void OnOutputDataReceived(string stdout) {
        SendConsoleEvent(OutputEvent.CategoryValue.Stdout, stdout);
    }
    public void OnErrorDataReceived(string stderr) {
        SendConsoleEvent(OutputEvent.CategoryValue.Stderr, stderr);
    }
    public void OnDebugDataReceived(string debug) {
        SendConsoleEvent(OutputEvent.CategoryValue.Console, debug);
    }

    private void SendConsoleEvent(OutputEvent.CategoryValue category, string message) {
        Protocol.SendEvent(new OutputEvent(message.Trim() + Environment.NewLine) {
            Category = category
        });
    }
    private void LogMessage(object? sender, LogEventArgs args) {
        DebuggerLoggingService.CustomLogger.LogMessage(args.Message);
    }
    private void LogError(object? sender, DispatcherErrorEventArgs args) {
        DebuggerLoggingService.CustomLogger.LogError($"[Fatal] {args.Exception.Message}", args.Exception);
        OnUnhandledException(args.Exception);
    }
}
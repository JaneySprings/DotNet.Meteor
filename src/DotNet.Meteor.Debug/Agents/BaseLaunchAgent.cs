using System;
using System.Collections.Generic;
using DotNet.Meteor.HotReload;
using DotNet.Meteor.Processes;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Mono.Debugging.Soft;
using DebuggerLoggingService = Mono.Debugging.Client.DebuggerLoggingService;

namespace DotNet.Meteor.Debug;

public abstract class BaseLaunchAgent {
    public const string CommandPrefix = "/";
    public const string LanguageSeparator = "!";

    protected List<Action> Disposables { get; init; }
    protected LaunchConfiguration Configuration { get; init; }
    protected HotReloadClient HotReloadClient { get; init; }

    protected BaseLaunchAgent(LaunchConfiguration configuration) {
        HotReloadClient = new HotReloadClient(configuration.ReloadHostPort, configuration.Device.IsAndroid);
        Disposables = new List<Action>();
        Configuration = configuration;
    }

    public abstract void Connect(SoftDebuggerSession session);
    public abstract void Launch(IProcessLogger logger);

    public virtual List<CompletionItem> GetCompletionItems() => new List<CompletionItem>();
    public virtual void HandleCommand(string command, IProcessLogger logger) { }
    public virtual void ConnectHotReload() {
        Disposables.Add(() => HotReloadClient.Close());
        _ = HotReloadClient.TryConnectAsync();
    }
    public virtual void SendHotReloadNotification(string filePath, IProcessLogger logger = null) {
        try {
            HotReloadClient.SendNotification(filePath, logger);
        } catch (Exception ex) {
            logger.OnErrorDataReceived($"[HotReload]: {ex.Message}");
        }
    }
    public virtual void Dispose() {
        foreach (var disposable in Disposables) {
            disposable.Invoke();
            DebuggerLoggingService.CustomLogger.LogMessage($"Disposing {disposable.Method.Name}");
        }

        Disposables.Clear();
    }
}
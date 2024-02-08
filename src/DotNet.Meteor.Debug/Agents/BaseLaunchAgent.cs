using System;
using System.Collections.Generic;
using DotNet.Meteor.Processes;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Mono.Debugging.Soft;
using NLog;

namespace DotNet.Meteor.Debug;

public abstract class BaseLaunchAgent {
    private readonly Logger sessionLogger = LogManager.GetCurrentClassLogger();
    
    public const string CommandPrefix = "/";
    public List<Action> Disposables { get; init; }
    protected LaunchConfiguration Configuration { get; init; }

    protected BaseLaunchAgent(LaunchConfiguration configuration) {
        Disposables = new List<Action>();
        Configuration = configuration;
    }

    public abstract void Connect(SoftDebuggerSession session);
    public abstract void Launch(IProcessLogger logger);
    
    public virtual List<CompletionItem> GetCompletionItems() => new List<CompletionItem>();
    public virtual void HandleCommand(string command, IProcessLogger logger) {}
    public virtual void Dispose() {
        foreach(var disposable in Disposables) {
            disposable.Invoke();
            sessionLogger.Debug($"Disposing {disposable.Method.Name}");
        }

        Disposables.Clear();
    }
}
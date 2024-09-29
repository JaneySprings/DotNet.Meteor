using DotNet.Meteor.Common.Processes;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Mono.Debugging.Soft;
using DebuggerLoggingService = Mono.Debugging.Client.DebuggerLoggingService;

namespace DotNet.Meteor.Debug;

public abstract class BaseLaunchAgent {
    public const string CommandPrefix = "/";
    public const string LanguageSeparator = "!";

    protected List<Action> Disposables { get; init; }
    protected LaunchConfiguration Configuration { get; init; }

    protected virtual string ProcessedCommand => string.Empty;

    protected BaseLaunchAgent(LaunchConfiguration configuration) {
        Disposables = new List<Action>();
        Configuration = configuration;
    }

    public abstract void Connect(SoftDebuggerSession session);
    public abstract void Launch(IProcessLogger logger);
    public virtual void HandleCommand(string command, string args, IProcessLogger logger) { }

    public void HandleCommand(string command, IProcessLogger logger) {
        if (string.IsNullOrEmpty(ProcessedCommand) || !command.StartsWith($"{CommandPrefix}{ProcessedCommand}", StringComparison.OrdinalIgnoreCase))
            return;

        var args = command.Replace($"{CommandPrefix}{ProcessedCommand}", string.Empty).Trim();
        HandleCommand(ProcessedCommand, args, logger);
    }
    public List<CompletionItem> GetCompletionItems() {
        if (string.IsNullOrEmpty(ProcessedCommand))
            return new List<CompletionItem>();

        return new List<CompletionItem>() {
            new CompletionItem() { Label = ProcessedCommand, Type = CompletionItemType.Snippet }
        };
    }
    public virtual void Dispose() {
        foreach (var disposable in Disposables) {
            try {
                disposable.Invoke();
                DebuggerLoggingService.CustomLogger.LogMessage($"Disposing {disposable.Method.Name}");
            } catch (Exception ex) {
                DebuggerLoggingService.CustomLogger.LogMessage($"Error while disposing {disposable.Method.Name}: {ex.Message}");
            }
        }

        Disposables.Clear();
    }
}
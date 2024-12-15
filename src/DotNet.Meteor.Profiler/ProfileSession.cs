using DotNet.Meteor.Profiler.Extensions;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;

namespace DotNet.Meteor.Profiler;

public class ProfileSession : Session {
    private BaseLaunchAgent launchAgent = null!;

    public ProfileSession(Stream input, Stream output) : base(input, output) {}

    protected override void OnUnhandledException(Exception ex) => launchAgent?.Dispose();

    protected override InitializeResponse HandleInitializeRequest(InitializeArguments arguments) {
        return new InitializeResponse() {
            SupportsTerminateRequest = true,
            SupportsCompletionsRequest = true,
            CompletionTriggerCharacters = new List<string> { 
                BaseLaunchAgent.CommandPrefix 
            },
        };
    }
    protected override LaunchResponse HandleLaunchRequest(LaunchArguments arguments) {
        return ServerExtensions.DoSafe(() => {
            var configuration = new LaunchConfiguration(arguments.ConfigurationProperties);
            launchAgent = configuration.GetLaunchAgent();
            launchAgent.Launch(this);
            return new LaunchResponse();
        });
    }
    protected override TerminateResponse HandleTerminateRequest(TerminateArguments arguments) {
        launchAgent?.Dispose();
        Protocol.SendEvent(new TerminatedEvent());
        return new TerminateResponse();
    }
    protected override DisconnectResponse HandleDisconnectRequest(DisconnectArguments arguments) {
        launchAgent?.Dispose();
        return new DisconnectResponse();
    }
    protected override EvaluateResponse HandleEvaluateRequest(EvaluateArguments arguments) {
        launchAgent?.HandleCommand(arguments.Expression, this);
        throw new ProtocolException($"command handled by {launchAgent}");
    }
    protected override CompletionsResponse HandleCompletionsRequest(CompletionsArguments arguments) {
        return new CompletionsResponse(launchAgent?.GetCompletionItems());
    }
}
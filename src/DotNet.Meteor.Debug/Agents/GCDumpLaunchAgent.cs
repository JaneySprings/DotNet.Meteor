using DotNet.Meteor.Processes;
using Mono.Debugging.Soft;

namespace DotNet.Meteor.Debug;

public class GCDumpLaunchAgent : BaseLaunchAgent {
    public GCDumpLaunchAgent(LaunchConfiguration configuration) : base(configuration) {}

    public override void Connect(SoftDebuggerSession session) {}
    public override void Launch(IProcessLogger logger) {
        // var nettracePath = Path.Combine(configuration.TempDirectoryPath, $"{configuration.GetApplicationName()}.nettrace");
        // var diagnosticPort = Path.Combine(RuntimeSystem.HomeDirectory, $"{configuration.Device.Platform}-port.lock");
        // ServerExtensions.TryDeleteFile(diagnosticPort);

        // if (configuration.Device.IsAndroid)
        //     LaunchAndroid(configuration, logger, diagnosticPort, nettracePath);
        // if (configuration.Device.IsIPhone)
        //     LaunchAppleMobile(configuration, logger, diagnosticPort, nettracePath);
        // if (configuration.Device.IsMacCatalyst)
        //     LaunchMacCatalyst(configuration, logger, diagnosticPort, nettracePath);
        // if (configuration.Device.IsWindows)
        //     LaunchWindows(configuration, logger, diagnosticPort, nettracePath);

        // Disposables.Add(() => ServerExtensions.TryDeleteFile(diagnosticPort));
    }

    private void LaunchAppleMobile(LaunchConfiguration configuration, IProcessLogger logger, string diagnosticPort, string nettracePath) {
    }
    private void LaunchMacCatalyst(LaunchConfiguration configuration, IProcessLogger logger, string diagnosticPort, string nettracePath) {
    }
    private void LaunchAndroid(LaunchConfiguration configuration, IProcessLogger logger, string diagnosticPort, string nettracePath) {
    }
    private void LaunchWindows(LaunchConfiguration configuration, IProcessLogger logger, string diagnosticPort, string nettracePath) {
    }
}
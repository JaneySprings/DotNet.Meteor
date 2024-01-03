using System;
using System.IO;
using DotNet.Meteor.Debug.Extensions;
using DotNet.Meteor.Debug.Sdk;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Mono.Debugging.Soft;

namespace DotNet.Meteor.Debug;

public class NoDebugLaunchAgent : BaseLaunchAgent {
    public override void Connect(SoftDebuggerSession session, LaunchConfiguration configuration) {}
    public override void Launch(LaunchConfiguration configuration, IProcessLogger logger) {
        if (configuration.Device.IsAndroid)
            LaunchAndroid(configuration, logger);
        if (configuration.Device.IsIPhone)
            LaunchAppleMobile(configuration, logger);
        if (configuration.Device.IsMacCatalyst)
            LaunchMacCatalyst(configuration, logger);
        if (configuration.Device.IsWindows)
            LaunchWindows(configuration, logger);
    }

    private void LaunchAppleMobile(LaunchConfiguration configuration, IProcessLogger logger) {
        if (configuration.Device.IsEmulator) {
            var debugProcess = MonoLaunch.DebugSim(configuration.Device.Serial, configuration.OutputAssembly, configuration.DebugPort, logger);
            Disposables.Add(() => debugProcess.Terminate());
        } else {
            MonoLaunch.InstallDev(configuration.Device.Serial, configuration.OutputAssembly, logger);
            var debugProcess = MonoLaunch.DebugDev(configuration.Device.Serial, configuration.OutputAssembly, configuration.DebugPort, logger);
            Disposables.Add(() => debugProcess.Terminate());
        }
    }
    private void LaunchMacCatalyst(LaunchConfiguration configuration, IProcessLogger logger) {
        var tool = AppleSdk.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder().AppendQuoted(configuration.OutputAssembly));
        var result = processRunner.WaitForExit();

        if (!result.Success)
            ServerExtensions.ThrowException(string.Join(Environment.NewLine, result.StandardError));
    }
    private void LaunchWindows(LaunchConfiguration configuration, IProcessLogger logger) {
        var program = new FileInfo(configuration.OutputAssembly);
        var process = new ProcessRunner(program, new ProcessArgumentBuilder(), logger).Start();
        Disposables.Add(() => process.Terminate());
    }
    private void LaunchAndroid(LaunchConfiguration configuration, IProcessLogger logger) {
        var applicationId = configuration.GetApplicationName();
        if (configuration.Device.IsEmulator)
            configuration.Device.Serial = AndroidEmulator.Run(configuration.Device.Name).Serial;

        if (configuration.ReloadHostPort > 0)
            DeviceBridge.Forward(configuration.Device.Serial, configuration.ReloadHostPort);

        if (configuration.UninstallApp)
            DeviceBridge.Uninstall(configuration.Device.Serial, applicationId, logger);

        DeviceBridge.Install(configuration.Device.Serial, configuration.OutputAssembly, logger);
        DeviceBridge.Launch(configuration.Device.Serial, applicationId, logger);
        DeviceBridge.Flush(configuration.Device.Serial);

        var logcatFirstChannelProcess = DeviceBridge.Logcat(configuration.Device.Serial, "system,crash", "*:I", logger);
        var logcatSecondChannelProcess = DeviceBridge.Logcat(configuration.Device.Serial, "main", "DOTNET:I", logger);

        Disposables.Add(() => logcatFirstChannelProcess.Terminate());
        Disposables.Add(() => logcatSecondChannelProcess.Terminate());
        Disposables.Add(() => DeviceBridge.RemoveForward(configuration.Device.Serial));
    }
}
using System;
using System.Net;
using DotNet.Meteor.Debug.Extensions;
using DotNet.Meteor.Debug.Sdb;
using DotNet.Meteor.Debug.Sdk;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Mono.Debugging.Soft;

namespace DotNet.Meteor.Debug;

public class DebugLaunchAgent : BaseLaunchAgent {
    public override void Connect(SoftDebuggerSession session, LaunchConfiguration configuration) {
        SoftDebuggerStartArgs arguments = null;

        if (configuration.Device.IsAndroid || (configuration.Device.IsIPhone && !configuration.Device.IsEmulator))
            arguments = new ClientConnectionProvider(IPAddress.Loopback, configuration.DebugPort, configuration.Project.Name);
        else if (configuration.Device.IsIPhone || configuration.Device.IsMacCatalyst)
            arguments = new ServerConnectionProvider(IPAddress.Loopback, configuration.DebugPort, configuration.Project.Name);

        ArgumentNullException.ThrowIfNull(arguments, "Debugger connection arguments not implemented.");
        session.Run(new SoftDebuggerStartInfo(arguments), configuration.DebuggerSessionOptions);
    }
    public override void Launch(LaunchConfiguration configuration, IProcessLogger logger) {
        if (configuration.Device.IsAndroid)
            LaunchAndroid(configuration, logger);
        if (configuration.Device.IsIPhone)
            LaunchAppleMobile(configuration, logger);
        if (configuration.Device.IsMacCatalyst)
            LaunchMacCatalyst(configuration, logger);
        if (configuration.Device.IsWindows)
            throw new NotSupportedException();
    }

    private void LaunchAppleMobile(LaunchConfiguration configuration, IProcessLogger logger) {
        // TODO: Implement Apple launching for Windows
        // if (RuntimeSystem.IsWindows) {
        //     IDeviceTool.Installer(configuration.Device.Serial, configuration.OutputAssembly, this);
            
        //     var debugProcess = IDeviceTool.Debug(configuration.Device.Serial, configuration.GetApplicationId(), configuration.DebugPort, this);
        //     disposables.Add(() => debugProcess.Kill());
        //     return;
        // }

        if (configuration.Device.IsEmulator) {
            var debugProcess = MonoLaunch.DebugSim(configuration.Device.Serial, configuration.OutputAssembly, configuration.DebugPort, logger);
            Disposables.Add(() => debugProcess.Terminate());
        } else {
            var forwardingProcess = MonoLaunch.TcpTunnel(configuration.Device.Serial, configuration.DebugPort, logger);
            MonoLaunch.InstallDev(configuration.Device.Serial, configuration.OutputAssembly, logger);
            
            var debugProcess = MonoLaunch.DebugDev(configuration.Device.Serial, configuration.OutputAssembly, configuration.DebugPort, logger);
            Disposables.Add(() => debugProcess.Terminate());
            Disposables.Add(() => forwardingProcess.Terminate());
        }
    }
    private void LaunchMacCatalyst(LaunchConfiguration configuration, IProcessLogger logger) {
        var tool = AppleSdk.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder().AppendQuoted(configuration.OutputAssembly));
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_HOSTS__", "127.0.0.1");
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_PORT__", configuration.DebugPort.ToString());
        var result = processRunner.WaitForExit();

        if (!result.Success)
            ServerExtensions.ThrowException(string.Join(Environment.NewLine, result.StandardError));
    }
    private void LaunchAndroid(LaunchConfiguration configuration, IProcessLogger logger) {
        var applicationId = configuration.GetApplicationName();
        if (configuration.Device.IsEmulator)
            configuration.Device.Serial = AndroidEmulator.Run(configuration.Device.Name).Serial;

        if (configuration.ReloadHostPort > 0)
            DeviceBridge.Forward(configuration.Device.Serial, configuration.ReloadHostPort);

        DeviceBridge.Forward(configuration.Device.Serial, configuration.DebugPort);

        if (configuration.UninstallApp)
            DeviceBridge.Uninstall(configuration.Device.Serial, applicationId, logger);

        DeviceBridge.Install(configuration.Device.Serial, configuration.OutputAssembly, logger);
        DeviceBridge.Shell(configuration.Device.Serial, "setprop", "debug.mono.connect", $"port={configuration.DebugPort}");
        DeviceBridge.Launch(configuration.Device.Serial, applicationId, logger);
        DeviceBridge.Flush(configuration.Device.Serial);

        var logcatFirstChannelProcess = DeviceBridge.Logcat(configuration.Device.Serial, "system,crash", "*:I", logger);
        var logcatSecondChannelProcess = DeviceBridge.Logcat(configuration.Device.Serial, "main", "DOTNET:I", logger);

        Disposables.Add(() => logcatFirstChannelProcess.Terminate());
        Disposables.Add(() => logcatSecondChannelProcess.Terminate());
        Disposables.Add(() => DeviceBridge.RemoveForward(configuration.Device.Serial));
    }
}
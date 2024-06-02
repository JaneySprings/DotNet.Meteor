using System;
using System.Net;
using DotNet.Meteor.Debug.Extensions;
using DotNet.Meteor.Debug.Sdb;
using DotNet.Meteor.Debug.Sdk;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;
using Mono.Debugging.Soft;

namespace DotNet.Meteor.Debug;

public class DebugLaunchAgent : BaseLaunchAgent {
    private readonly SoftDebuggerStartArgs startArguments;
    private readonly SoftDebuggerStartInfo startInformation;

    public DebugLaunchAgent(LaunchConfiguration configuration) : base(configuration) {
        if (configuration.Device.IsAndroid || (configuration.Device.IsIPhone && !configuration.Device.IsEmulator))
            startArguments = new ClientConnectionProvider(IPAddress.Loopback, configuration.DebugPort, configuration.Project.Name);
        else if (configuration.Device.IsIPhone || configuration.Device.IsMacCatalyst)
            startArguments = new ServerConnectionProvider(IPAddress.Loopback, configuration.DebugPort, configuration.Project.Name);

        ArgumentNullException.ThrowIfNull(startArguments, "Debugger connection arguments not implemented.");

        startInformation = new SoftDebuggerStartInfo(startArguments);
        startInformation.SetAssemblies(configuration.GetApplicationAssembliesDirectory(), configuration.DebuggerSessionOptions);
    }
    public override void Launch(IProcessLogger logger) {
        if (Configuration.Device.IsAndroid)
            LaunchAndroid(logger);
        if (Configuration.Device.IsIPhone)
            LaunchAppleMobile(logger);
        if (Configuration.Device.IsMacCatalyst)
            LaunchMacCatalyst(logger);
        if (Configuration.Device.IsWindows)
            throw new NotSupportedException();
    }
    public override void Connect(SoftDebuggerSession session) {
        session.Run(startInformation, Configuration.DebuggerSessionOptions);
    }

    private void LaunchAppleMobile(IProcessLogger logger) {
        // TODO: Implement Apple launching for Windows
        // if (RuntimeSystem.IsWindows) {
        //     IDeviceTool.Installer(Configuration.Device.Serial, Configuration.OutputAssembly, this);

        //     var debugProcess = IDeviceTool.Debug(Configuration.Device.Serial, Configuration.GetApplicationId(), Configuration.DebugPort, this);
        //     disposables.Add(() => debugProcess.Kill());
        //     return;
        // }

        if (Configuration.Device.IsEmulator) {
            var debugProcess = MonoLaunch.DebugSim(Configuration.Device.Serial, Configuration.OutputAssembly, Configuration.DebugPort, logger);
            Disposables.Add(() => debugProcess.Terminate());
        } else {
            var debugPortForwarding = MonoLaunch.TcpTunnel(Configuration.Device.Serial, Configuration.DebugPort, logger);
            var hotReloadPortForwarding = MonoLaunch.TcpTunnel(Configuration.Device.Serial, Configuration.ReloadHostPort, logger);
            MonoLaunch.InstallDev(Configuration.Device.Serial, Configuration.OutputAssembly, logger);

            var debugProcess = MonoLaunch.DebugDev(Configuration.Device.Serial, Configuration.OutputAssembly, Configuration.DebugPort, logger);
            Disposables.Add(() => debugProcess.Terminate());
            Disposables.Add(() => debugPortForwarding.Terminate());
            Disposables.Add(() => hotReloadPortForwarding.Terminate());
        }
    }
    private void LaunchMacCatalyst(IProcessLogger logger) {
        var tool = AppleSdk.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder().AppendQuoted(Configuration.OutputAssembly));
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_HOSTS__", "127.0.0.1");
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_PORT__", Configuration.DebugPort.ToString());
        var result = processRunner.WaitForExit();

        if (!result.Success)
            throw ServerExtensions.GetProtocolException(string.Join(Environment.NewLine, result.StandardError));
    }
    private void LaunchAndroid(IProcessLogger logger) {
        var applicationId = Configuration.GetApplicationName();
        if (Configuration.Device.IsEmulator)
            Configuration.Device.Serial = AndroidEmulator.Run(Configuration.Device.Name).Serial;

        DeviceBridge.Forward(Configuration.Device.Serial, Configuration.ReloadHostPort);
        DeviceBridge.Forward(Configuration.Device.Serial, Configuration.DebugPort);

        if (Configuration.UninstallApp)
            DeviceBridge.Uninstall(Configuration.Device.Serial, applicationId, logger);

        DeviceBridge.Install(Configuration.Device.Serial, Configuration.OutputAssembly, logger);
        DeviceBridge.Shell(Configuration.Device.Serial, "setprop", "debug.mono.connect", $"port={Configuration.DebugPort}");
        DeviceBridge.Shell(Configuration.Device.Serial, "am", "set-debug-app", applicationId);
        DeviceBridge.Launch(Configuration.Device.Serial, applicationId, logger);
        DeviceBridge.Flush(Configuration.Device.Serial);

        var logcatProcess = DeviceBridge.Logcat(Configuration.Device.Serial, logger);

        Disposables.Add(() => logcatProcess.Terminate());
        Disposables.Add(() => DeviceBridge.RemoveForward(Configuration.Device.Serial));
    }
}
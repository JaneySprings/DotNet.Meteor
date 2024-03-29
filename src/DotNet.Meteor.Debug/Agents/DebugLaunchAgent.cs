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
    public DebugLaunchAgent(LaunchConfiguration configuration) : base(configuration) {}

    public override void Connect(SoftDebuggerSession session) {
        SoftDebuggerStartArgs arguments = null;

        if (Configuration.Device.IsAndroid || (Configuration.Device.IsIPhone && !Configuration.Device.IsEmulator))
            arguments = new ClientConnectionProvider(IPAddress.Loopback, Configuration.DebugPort, Configuration.Project.Name);
        else if (Configuration.Device.IsIPhone || Configuration.Device.IsMacCatalyst)
            arguments = new ServerConnectionProvider(IPAddress.Loopback, Configuration.DebugPort, Configuration.Project.Name);

        ArgumentNullException.ThrowIfNull(arguments, "Debugger connection arguments not implemented.");
        session.Run(new SoftDebuggerStartInfo(arguments), Configuration.DebuggerSessionOptions);
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
            MonoLaunch.InstallDev(Configuration.Device.Serial, Configuration.OutputAssembly, logger);
            
            var debugProcess = MonoLaunch.DebugDev(Configuration.Device.Serial, Configuration.OutputAssembly, Configuration.DebugPort, logger);
            Disposables.Add(() => debugProcess.Terminate());
            Disposables.Add(() => debugPortForwarding.Terminate());
        }
    }
    private void LaunchMacCatalyst(IProcessLogger logger) {
        var tool = AppleSdk.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder().AppendQuoted(Configuration.OutputAssembly));
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_HOSTS__", "127.0.0.1");
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_PORT__", Configuration.DebugPort.ToString());
        var result = processRunner.WaitForExit();

        if (!result.Success)
            ServerExtensions.ThrowException(string.Join(Environment.NewLine, result.StandardError));
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
        DeviceBridge.Launch(Configuration.Device.Serial, applicationId, logger);
        DeviceBridge.Flush(Configuration.Device.Serial);

        var logcatFirstChannelProcess = DeviceBridge.Logcat(Configuration.Device.Serial, "system,crash", "*:I", logger);
        var logcatSecondChannelProcess = DeviceBridge.Logcat(Configuration.Device.Serial, "main", "DOTNET:I", logger);

        Disposables.Add(() => logcatFirstChannelProcess.Terminate());
        Disposables.Add(() => logcatSecondChannelProcess.Terminate());
        Disposables.Add(() => DeviceBridge.RemoveForward(Configuration.Device.Serial));
    }
}
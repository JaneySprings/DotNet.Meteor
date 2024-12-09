using System.Net;
using DotNet.Meteor.Debug.Extensions;
using DotNet.Meteor.Debug.Sdb;
using DotNet.Meteor.Common;
using Mono.Debugging.Soft;
using DotNet.Meteor.Common.Processes;
using DotNet.Meteor.Common.Apple;
using DotNet.Meteor.Common.Android;

namespace DotNet.Meteor.Debug;

public class DebugLaunchAgent : BaseLaunchAgent {
    private readonly SoftDebuggerStartArgs startArguments;
    private readonly SoftDebuggerStartInfo startInformation;
    private readonly ExternalTypeResolver typeResolver;

    public DebugLaunchAgent(LaunchConfiguration configuration) : base(configuration) {
        if (configuration.Device.IsAndroid || (configuration.Device.IsIPhone && !configuration.Device.IsEmulator))
            startArguments = new ClientConnectionProvider(IPAddress.Loopback, configuration.DebugPort, configuration.Project.Name);
        else if (configuration.Device.IsIPhone || configuration.Device.IsMacCatalyst)
            startArguments = new ServerConnectionProvider(IPAddress.Loopback, configuration.DebugPort, configuration.Project.Name);

        ArgumentNullException.ThrowIfNull(startArguments, "Debugger connection arguments not implemented.");

        typeResolver = new ExternalTypeResolver(configuration.TransportId);
        startInformation = new SoftDebuggerStartInfo(startArguments);
        startInformation.SetAssemblies(configuration.GetAssembliesPath(), configuration.DebuggerSessionOptions);
    }
    public override void Launch(DebugSession debugSession) {
        if (Configuration.Device.IsAndroid)
            LaunchAndroid(debugSession);
        if (Configuration.Device.IsIPhone)
            LaunchAppleMobile(debugSession);
        if (Configuration.Device.IsMacCatalyst)
            LaunchMacCatalyst(debugSession);
        if (Configuration.Device.IsWindows)
            throw new NotSupportedException();
    }
    public override void Connect(SoftDebuggerSession session) {
        session.Run(startInformation, Configuration.DebuggerSessionOptions);
        if (typeResolver.TryConnect()) {
            Disposables.Add(() => typeResolver.Dispose());
            session.TypeResolverHandler = typeResolver.Resolve;
        }
    }

    private void LaunchAppleMobile(DebugSession debugSession) {
        if (RuntimeSystem.IsMacOS) {
            if (Configuration.Device.IsEmulator) {
                var debugProcess = MonoLauncher.DebugSim(Configuration.Device.Serial, Configuration.ProgramPath, Configuration.DebugPort, Configuration.EnvironmentVariables, debugSession);
                Disposables.Add(() => debugProcess.Terminate());
            } else {
                var debugPortForwarding = MonoLauncher.TcpTunnel(Configuration.Device.Serial, Configuration.DebugPort, debugSession);
                var hotReloadPortForwarding = MonoLauncher.TcpTunnel(Configuration.Device.Serial, Configuration.ReloadHostPort, debugSession);
                MonoLauncher.InstallDev(Configuration.Device.Serial, Configuration.ProgramPath, debugSession);

                var debugProcess = MonoLauncher.DebugDev(Configuration.Device.Serial, Configuration.ProgramPath, Configuration.DebugPort, Configuration.EnvironmentVariables, debugSession);
                Disposables.Add(() => debugProcess.Terminate());
                Disposables.Add(() => debugPortForwarding.Terminate());
                Disposables.Add(() => hotReloadPortForwarding.Terminate());
            }
        } else {
            var debugProxyProcess = IDeviceTool.Proxy(Configuration.Device.Serial, Configuration.DebugPort, debugSession);
            Disposables.Add(() => debugProxyProcess.Terminate());
            var reloadProxyProcess = IDeviceTool.Proxy(Configuration.Device.Serial, Configuration.ReloadHostPort, debugSession);
            Disposables.Add(() => debugProxyProcess.Terminate());

            IDeviceTool.Installer(Configuration.Device.Serial, Configuration.ProgramPath, debugSession);
            debugSession.OnImportantDataReceived("Application installed on device. Tap the application icon on your device to run it.");
        }
    }
    private void LaunchMacCatalyst(IProcessLogger logger) {
        var tool = AppleSdkLocator.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder().AppendQuoted(Configuration.ProgramPath));
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_HOSTS__", "127.0.0.1");
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_PORT__", Configuration.DebugPort.ToString());
        foreach (var env in Configuration.EnvironmentVariables)
            processRunner.SetEnvironmentVariable(env.Key, env.Value);

        var result = processRunner.WaitForExit();
        if (!result.Success)
            throw ServerExtensions.GetProtocolException(string.Join(Environment.NewLine, result.StandardError));
    }
    private void LaunchAndroid(IProcessLogger logger) {
        var applicationId = Configuration.GetApplicationName();
        if (Configuration.Device.IsEmulator)
            Configuration.Device.Serial = AndroidEmulator.Run(Configuration.Device.Name).Serial;

        AndroidDebugBridge.Forward(Configuration.Device.Serial, Configuration.ReloadHostPort);
        AndroidDebugBridge.Forward(Configuration.Device.Serial, Configuration.DebugPort);

        if (Configuration.UninstallApp)
            AndroidDebugBridge.Uninstall(Configuration.Device.Serial, applicationId, logger);

        AndroidDebugBridge.Install(Configuration.Device.Serial, Configuration.ProgramPath, logger);
        AndroidDebugBridge.Shell(Configuration.Device.Serial, "setprop", "debug.mono.connect", $"port={Configuration.DebugPort}");
        if (Configuration.EnvironmentVariables.Count != 0)
            AndroidDebugBridge.Shell(Configuration.Device.Serial, "setprop", "debug.mono.env", Configuration.EnvironmentVariables.ToEnvString());
        
        AndroidDebugBridge.Shell(Configuration.Device.Serial, "am", "set-debug-app", applicationId);

        AndroidFastDev.TryPushAssemblies(Configuration.Device, Configuration.AssetsPath, applicationId, logger);

        AndroidDebugBridge.Launch(Configuration.Device.Serial, applicationId, logger);
        AndroidDebugBridge.Flush(Configuration.Device.Serial);

        var logcatProcess = AndroidDebugBridge.Logcat(Configuration.Device.Serial, logger);

        Disposables.Add(() => logcatProcess.Terminate());
        Disposables.Add(() => AndroidDebugBridge.RemoveForward(Configuration.Device.Serial));
    }
}
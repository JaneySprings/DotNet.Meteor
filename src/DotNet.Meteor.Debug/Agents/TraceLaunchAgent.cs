using DotNet.Meteor.Debug.Extensions;
using DotNet.Meteor.Common;
using Mono.Debugging.Soft;
using DotNet.Meteor.Common.Processes;
using DotNet.Meteor.Common.Apple;
using DotNet.Meteor.Common.Android;
using DotNet.Meteor.Debug.Tools;

namespace DotNet.Meteor.Debug;

public class TraceLaunchAgent : BaseLaunchAgent {
    public TraceLaunchAgent(LaunchConfiguration configuration) : base(configuration) { }
    public override void Connect(SoftDebuggerSession session) { }
    public override void Launch(IProcessLogger logger) {
        var nettracePath = Path.Combine(Path.GetDirectoryName(Configuration.Project.Path)!, $"{Configuration.GetApplicationName()}.nettrace");
        var diagnosticPort = Path.Combine(RuntimeSystem.HomeDirectory, $"{Configuration.Device.Platform}-port.lock");
        ServerExtensions.TryDeleteFile(diagnosticPort);

        if (Configuration.Device.IsAndroid)
            LaunchAndroid(logger, diagnosticPort, nettracePath);
        if (Configuration.Device.IsIPhone)
            LaunchAppleMobile(logger, diagnosticPort, nettracePath);
        if (Configuration.Device.IsMacCatalyst)
            LaunchMacCatalyst(logger, diagnosticPort, nettracePath);
        if (Configuration.Device.IsWindows)
            LaunchWindows(logger, diagnosticPort, nettracePath);

        Disposables.Add(() => ServerExtensions.TryDeleteFile(diagnosticPort));
    }

    private void LaunchAppleMobile(IProcessLogger logger, string diagnosticPort, string nettracePath) {
        if (Configuration.Device.IsEmulator) {
            var routerProcess = DSRouter.ClientToServer(diagnosticPort, $"127.0.0.1:{Configuration.ProfilerPort}", logger);
            var simProcess = MonoLauncher.ProfileSim(Configuration.Device.Serial, Configuration.ProgramPath, $"127.0.0.1:{Configuration.ProfilerPort},suspend", new CatchStartLogger(logger, () => {
                var traceProcess = Trace.Collect(diagnosticPort, nettracePath, logger);
                Disposables.Insert(0, () => traceProcess.Terminate());
            }));

            Disposables.Add(() => routerProcess.Terminate());
            Disposables.Add(() => simProcess.Terminate());
        } else {
            var routerProcess = DSRouter.ServerToClient(diagnosticPort, $"127.0.0.1:{Configuration.ProfilerPort}", forwardApple: true, logger);
            MonoLauncher.InstallDev(Configuration.Device.Serial, Configuration.ProgramPath, logger);
            var devProcess = MonoLauncher.ProfileDev(Configuration.Device.Serial, Configuration.ProgramPath, $"127.0.0.1:{Configuration.ProfilerPort},suspend,listen", new CatchStartLogger(logger, () => {
                var traceProcess = Trace.Collect($"{diagnosticPort},connect", nettracePath, logger);
                Disposables.Insert(0, () => traceProcess.Terminate());
            }));

            Disposables.Add(() => routerProcess.Terminate());
            Disposables.Add(() => devProcess.Terminate());
        }
    }
    private void LaunchMacCatalyst(IProcessLogger logger, string diagnosticPort, string nettracePath) {
        var tool = AppleSdkLocator.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder().AppendQuoted(Configuration.ProgramPath));
        processRunner.SetEnvironmentVariable("DOTNET_DiagnosticPorts", $"{diagnosticPort},suspend");

        var traceProcess = Trace.Collect(diagnosticPort, nettracePath, logger);
        var appLaunchResult = processRunner.WaitForExit();

        Disposables.Add(() => traceProcess.Terminate());

        if (!appLaunchResult.Success)
            throw new InvalidOperationException(string.Join(Environment.NewLine, appLaunchResult.StandardError));
    }
    private void LaunchAndroid(IProcessLogger logger, string diagnosticPort, string nettracePath) {
        var applicationId = Configuration.GetApplicationName();
        if (Configuration.Device.IsEmulator)
            Configuration.Device.Serial = AndroidEmulator.Run(Configuration.Device.Name).Serial;

        AndroidDebugBridge.Reverse(Configuration.Device.Serial, Configuration.ProfilerPort, Configuration.ProfilerPort + 1);
        AndroidDebugBridge.Shell(Configuration.Device.Serial, "setprop", "debug.mono.profile", $"127.0.0.1:{Configuration.ProfilerPort},suspend");

        var routerProcess = DSRouter.ServerToServer(Configuration.ProfilerPort + 1, logger);
        Disposables.Add(() => routerProcess.Terminate());
        Disposables.Add(() => AndroidDebugBridge.RemoveReverse(Configuration.Device.Serial));

        if (Configuration.UninstallApp)
            AndroidDebugBridge.Uninstall(Configuration.Device.Serial, applicationId, logger);
        AndroidDebugBridge.Install(Configuration.Device.Serial, Configuration.ProgramPath, logger);
        AndroidDebugBridge.Launch(Configuration.Device.Serial, applicationId, logger);

        var traceProcess = Trace.Collect(routerProcess.Id, nettracePath, logger);
        Disposables.Insert(0, () => traceProcess.Terminate());
        Disposables.Add(() => AndroidDebugBridge.Shell(Configuration.Device.Serial, "am", "force-stop", applicationId));
    }
    private void LaunchWindows(IProcessLogger logger, string diagnosticPort, string nettracePath) {
        var exeProcess = new ProcessRunner(new FileInfo(Configuration.ProgramPath), null, logger).Start();
        var traceProcess = Trace.Collect(exeProcess.Id, nettracePath, logger);

        Disposables.Add(() => traceProcess.Terminate());
        Disposables.Add(() => exeProcess.Terminate());
    }
}
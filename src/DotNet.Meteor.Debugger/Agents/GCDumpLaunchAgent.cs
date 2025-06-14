using DotNet.Meteor.Common;
using Mono.Debugging.Soft;
using DotNet.Meteor.Common.Processes;
using DotNet.Meteor.Common.Apple;
using DotNet.Meteor.Common.Android;
using DotNet.Meteor.Debugger.Tools;
using DotNet.Meteor.Debugger.Extensions;

namespace DotNet.Meteor.Debugger;

public class GCDumpLaunchAgent : BaseLaunchAgent {
    private string diagnosticPort = null!;
    private string gcdumpPath = null!;
    private int applicationPID;

    protected override string ProcessedCommand => "dump";

    public GCDumpLaunchAgent(LaunchConfiguration configuration) : base(configuration) { }
    public override void Connect(SoftDebuggerSession session) { }
    public override void Launch(DebugSession debugSession) {
        gcdumpPath = Path.Combine(Path.GetDirectoryName(Configuration.Project.Path)!, $"{Configuration.GetApplicationName()}.gcdump");
        diagnosticPort = Path.Combine(RuntimeSystem.HomeDirectory, $"{Configuration.Device.Platform}-port.lock");
        ServerExtensions.TryDeleteFile(diagnosticPort);

        if (Configuration.Device.IsAndroid)
            LaunchAndroid(debugSession);
        if (Configuration.Device.IsIPhone)
            LaunchAppleMobile(debugSession);
        if (Configuration.Device.IsMacCatalyst)
            LaunchMacCatalyst(debugSession);
        if (Configuration.Device.IsWindows)
            LaunchWindows(debugSession);

        Disposables.Add(() => ServerExtensions.TryDeleteFile(diagnosticPort));
    }
    public override void HandleCommand(string command, string args, IProcessLogger logger) {
        if (string.IsNullOrEmpty(diagnosticPort) || string.IsNullOrEmpty(gcdumpPath))
            return;

        var gcdumpProcess = applicationPID == 0
            ? GCDump.Collect($"{diagnosticPort},connect", gcdumpPath, args, logger)
            : GCDump.Collect(applicationPID, gcdumpPath, args, logger);

        Disposables.Insert(0, () => gcdumpProcess.Terminate());
    }

    private void LaunchAppleMobile(IProcessLogger logger) {
        if (Configuration.Device.IsEmulator) {
            var routerProcess = DSRouter.ServerToClient($"{diagnosticPort}", $"127.0.0.1:{Configuration.ProfilerPort}", false, logger);
            var simProcess = MonoLauncher.ProfileSim(Configuration.Device.Serial, Configuration.ProgramPath, $"127.0.0.1:{Configuration.ProfilerPort},nosuspend,listen", logger);

            Disposables.Add(() => routerProcess.Terminate());
            Disposables.Add(() => simProcess.Terminate());
        } else {
            var routerProcess = DSRouter.ServerToClient(diagnosticPort, $"127.0.0.1:{Configuration.ProfilerPort}", forwardApple: true, logger);
            MonoLauncher.InstallDev(Configuration.Device.Serial, Configuration.ProgramPath, logger);
            var devProcess = MonoLauncher.ProfileDev(Configuration.Device.Serial, Configuration.ProgramPath, $"127.0.0.1:{Configuration.ProfilerPort},nosuspend,listen", logger);

            Disposables.Add(() => routerProcess.Terminate());
            Disposables.Add(() => devProcess.Terminate());
        }
    }
    private void LaunchMacCatalyst(IProcessLogger logger) {
        var tool = AppleSdkLocator.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder().AppendQuoted(Configuration.ProgramPath));

        diagnosticPort = $"127.0.0.1:{Configuration.ProfilerPort}";
        processRunner.SetEnvironmentVariable("DOTNET_DiagnosticPorts", $"127.0.0.1:{Configuration.ProfilerPort},nosuspend,listen");

        var appLaunchResult = processRunner.WaitForExit();
        if (!appLaunchResult.Success)
            throw new InvalidOperationException(string.Join(Environment.NewLine, appLaunchResult.StandardError));
    }
    private void LaunchAndroid(IProcessLogger logger) {
        var applicationId = Configuration.GetApplicationName();
        if (Configuration.Device.IsEmulator)
            Configuration.Device.Serial = AndroidEmulator.Run(Configuration.Device.Name).Serial;

        AndroidDebugBridge.Reverse(Configuration.Device.Serial, Configuration.ProfilerPort, Configuration.ProfilerPort + 1);
        AndroidDebugBridge.Shell(Configuration.Device.Serial, "setprop", "debug.mono.profile", $"127.0.0.1:{Configuration.ProfilerPort},nosuspend,connect");

        var routerProcess = DSRouter.ServerToServer(Configuration.ProfilerPort + 1, logger);
        applicationPID = routerProcess.Id;

        Disposables.Add(() => routerProcess.Terminate());
        Disposables.Add(() => AndroidDebugBridge.RemoveReverse(Configuration.Device.Serial));

        if (Configuration.UninstallApp)
            AndroidDebugBridge.Uninstall(Configuration.Device.Serial, applicationId, logger);
        AndroidDebugBridge.Install(Configuration.Device.Serial, Configuration.ProgramPath, logger);
        AndroidDebugBridge.Launch(Configuration.Device.Serial, applicationId, logger);

        Disposables.Add(() => AndroidDebugBridge.Shell(Configuration.Device.Serial, "am", "force-stop", applicationId));
    }
    private void LaunchWindows(IProcessLogger logger) {
        var exeProcess = new ProcessRunner(new FileInfo(Configuration.ProgramPath), null, logger).Start();
        applicationPID = exeProcess.Id;

        Disposables.Add(() => exeProcess.Terminate());
    }
}
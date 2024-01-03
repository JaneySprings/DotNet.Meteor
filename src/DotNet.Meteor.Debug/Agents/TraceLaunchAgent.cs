using System;
using System.IO;
using DotNet.Meteor.Debug.Extensions;
using DotNet.Meteor.Debug.Sdk;
using DotNet.Meteor.Debug.Sdk.Profiling;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;
using Mono.Debugging.Soft;

namespace DotNet.Meteor.Debug;

public class TraceLaunchAgent : BaseLaunchAgent {
    public override void Connect(SoftDebuggerSession session, LaunchConfiguration configuration) {}
    public override void Launch(LaunchConfiguration configuration, IProcessLogger logger) {
        var nettracePath = Path.Combine(configuration.TempDirectoryPath, $"{configuration.GetApplicationName()}.nettrace");
        var diagnosticPort = Path.Combine(RuntimeSystem.HomeDirectory, $"{configuration.Device.Platform}-port.lock");
        ServerExtensions.TryDeleteFile(diagnosticPort);

        if (configuration.Device.IsAndroid)
            LaunchAndroid(configuration, logger, diagnosticPort, nettracePath);
        if (configuration.Device.IsIPhone)
            LaunchAppleMobile(configuration, logger, diagnosticPort, nettracePath);
        if (configuration.Device.IsMacCatalyst)
            LaunchMacCatalyst(configuration, logger, diagnosticPort, nettracePath);
        if (configuration.Device.IsWindows)
            LaunchWindows(configuration, logger, diagnosticPort, nettracePath);

        Disposables.Add(() => ServerExtensions.TryDeleteFile(diagnosticPort));
    }

    private void LaunchAppleMobile(LaunchConfiguration configuration, IProcessLogger logger, string diagnosticPort, string nettracePath) {
        if (configuration.Device.IsEmulator) {
            var routerProcess = DSRouter.ClientToServer(diagnosticPort, $"127.0.0.1:{configuration.ProfilerPort}", logger);
            var simProcess = MonoLaunch.ProfileSim(configuration.Device.Serial, configuration.OutputAssembly, configuration.ProfilerPort, new CatchStartLogger(logger, () => {
                var traceProcess = Trace.Collect(diagnosticPort, nettracePath, configuration.ProfilerMode, logger);
                Disposables.Insert(0, () => traceProcess.Terminate());
            }));
           
            Disposables.Add(() => routerProcess.Terminate());
            Disposables.Add(() => simProcess.Terminate());
        } else {
            var routerProcess = DSRouter.ServerToClient(diagnosticPort, $"127.0.0.1:{configuration.ProfilerPort}", forwardApple: true, logger);
            MonoLaunch.InstallDev(configuration.Device.Serial, configuration.OutputAssembly, logger);
            var devProcess = MonoLaunch.ProfileDev(configuration.Device.Serial, configuration.OutputAssembly, configuration.ProfilerPort, new CatchStartLogger(logger, () => {
                var traceProcess = Trace.Collect($"{diagnosticPort},connect", nettracePath, configuration.ProfilerMode, logger);
                Disposables.Insert(0, () => traceProcess.Terminate());
            }));
    
            Disposables.Add(() => routerProcess.Terminate());
            Disposables.Add(() => devProcess.Terminate());
        }
    }
    private void LaunchMacCatalyst(LaunchConfiguration configuration, IProcessLogger logger, string diagnosticPort, string nettracePath) {
        var tool = AppleSdk.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder().AppendQuoted(configuration.OutputAssembly));
        processRunner.SetEnvironmentVariable("DOTNET_DiagnosticPorts", $"{diagnosticPort},suspend");
        
        var traceProcess = Trace.Collect(diagnosticPort, nettracePath, configuration.ProfilerMode, logger);
        var appLaunchResult = processRunner.WaitForExit();

        Disposables.Add(() => traceProcess.Terminate());

        if (!appLaunchResult.Success)
            throw new Exception(string.Join(Environment.NewLine, appLaunchResult.StandardError));
    }
    private void LaunchAndroid(LaunchConfiguration configuration, IProcessLogger logger, string diagnosticPort, string nettracePath) {
        var applicationId = configuration.GetApplicationName();
        if (configuration.Device.IsEmulator)
            configuration.Device.Serial = AndroidEmulator.Run(configuration.Device.Name).Serial;

        DeviceBridge.Reverse(configuration.Device.Serial, configuration.ProfilerPort, configuration.ProfilerPort+1);
        DeviceBridge.Shell(configuration.Device.Serial, "setprop", "debug.mono.profile", $"127.0.0.1:{configuration.ProfilerPort},suspend");
        
        var routerProcess = DSRouter.ServerToServer(configuration.ProfilerPort+1, logger);
        System.Threading.Thread.Sleep(1000); // wait for router to start
        var traceProcess = Trace.Collect(routerProcess.Id, nettracePath, configuration.ProfilerMode, logger);
        System.Threading.Thread.Sleep(1000); // wait for trace to start

        if (configuration.UninstallApp)
            DeviceBridge.Uninstall(configuration.Device.Serial, applicationId, logger);
        DeviceBridge.Install(configuration.Device.Serial, configuration.OutputAssembly, logger);
        DeviceBridge.Launch(configuration.Device.Serial, applicationId, logger);

        Disposables.Add(() => traceProcess.Terminate());
        Disposables.Add(() => routerProcess.Terminate());
        Disposables.Add(() => DeviceBridge.Shell(configuration.Device.Serial, "am", "force-stop", applicationId));
        Disposables.Add(() => DeviceBridge.RemoveReverse(configuration.Device.Serial));
    }
    private void LaunchWindows(LaunchConfiguration configuration, IProcessLogger logger, string diagnosticPort, string nettracePath) {
        var exeProcess = new ProcessRunner(new FileInfo(configuration.OutputAssembly), null, logger).Start();
        var traceProcess = Trace.Collect(exeProcess.Id, nettracePath, configuration.ProfilerMode, logger);

        Disposables.Add(() => traceProcess.Terminate());
        Disposables.Add(() => exeProcess.Terminate());
    }
}
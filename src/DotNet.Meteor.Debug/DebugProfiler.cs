using System;
using System.IO;
using DotNet.Meteor.Shared;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Debug.Sdk;
using DotNet.Meteor.Debug.Sdk.Profiling;
using DotNet.Meteor.Debug.Extensions;
using System.Threading;

namespace DotNet.Meteor.Debug;

public partial class DebugSession {
    private void ProfileApplication(LaunchConfiguration configuration) {
        DoSafe(() => {
            if (configuration.Device.IsAndroid)
                ProfileAndroid(configuration);

            if (configuration.Device.IsIPhone)
                ProfileApple(configuration);

            if (configuration.Device.IsMacCatalyst)
                ProfileMacCatalyst(configuration);

            if (configuration.Device.IsWindows)
                ProfileWindows(configuration);
        });
    }

    private void ProfileApple(LaunchConfiguration configuration) {
        var applicationName = Path.GetFileNameWithoutExtension(configuration.OutputAssembly);
        var resultFilePath = Path.Combine(configuration.TempDirectoryPath, $"{applicationName}.nettrace");

        if (configuration.Device.IsEmulator) {
            var diagnosticPort = Path.Combine(RuntimeSystem.HomeDirectory, "simulator-port");
            var routerProcess = DSRouter.ClientToServer(diagnosticPort, $"127.0.0.1:{configuration.ProfilerPort}", this);
            var simProcess = MonoLaunch.ProfileSim(configuration.Device.Serial, configuration.OutputAssembly, configuration.ProfilerPort, new CatchStartLogger(this, () => {
                var traceProcess = Trace.Collect(diagnosticPort, resultFilePath, configuration.ProfilerMode, this);
                disposables.Insert(0, () => traceProcess.Terminate());
            }));
           
            disposables.Add(() => routerProcess.Terminate());
            disposables.Add(() => simProcess.Terminate());
        } else {
            var diagnosticPort = Path.Combine(RuntimeSystem.HomeDirectory, $"device-{DateTime.Now.Ticks}"); // Because after first try it shows 'Address already in use'
            var routerProcess = DSRouter.ServerToClient(diagnosticPort, $"127.0.0.1:{configuration.ProfilerPort}", forwardApple: true, this);

            MonoLaunch.InstallDev(configuration.Device.Serial, configuration.OutputAssembly, this);
            var devProcess = MonoLaunch.ProfileDev(configuration.Device.Serial, configuration.OutputAssembly, configuration.ProfilerPort, new CatchStartLogger(this, () => {
                var traceProcess = Trace.Collect($"{diagnosticPort},connect", resultFilePath, configuration.ProfilerMode, this);
                disposables.Insert(0, () => traceProcess.Terminate());
            }));
    
            disposables.Add(() => routerProcess.Terminate());
            disposables.Add(() => devProcess.Terminate());
        }
    }

    private void ProfileMacCatalyst(LaunchConfiguration configuration) {
        var applicationName = Path.GetFileNameWithoutExtension(configuration.OutputAssembly);
        var resultFilePath = Path.Combine(configuration.TempDirectoryPath, $"{applicationName}.nettrace");
        var diagnosticPort = Path.Combine(RuntimeSystem.HomeDirectory, "desktop-port");

        var tool = AppleSdk.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder().AppendQuoted(configuration.OutputAssembly));
        processRunner.SetEnvironmentVariable("DOTNET_DiagnosticPorts", $"{diagnosticPort},suspend");
        
        var traceProcess = Trace.Collect(diagnosticPort, resultFilePath, configuration.ProfilerMode, this);
        var appLaunchResult = processRunner.WaitForExit();

        disposables.Add(() => traceProcess.Terminate());

        if (!appLaunchResult.Success)
            throw new Exception(string.Join(Environment.NewLine, appLaunchResult.StandardError));
    }

    private void ProfileAndroid(LaunchConfiguration configuration) {
        var applicationId = configuration.GetApplicationId();
        var resultFilePath = Path.Combine(configuration.TempDirectoryPath, $"{applicationId}.nettrace");
        if (configuration.Device.IsEmulator)
            configuration.Device.Serial = AndroidEmulator.Run(configuration.Device.Name).Serial;

        DeviceBridge.Reverse(configuration.Device.Serial, configuration.ProfilerPort, configuration.ProfilerPort+1);
        DeviceBridge.Shell(configuration.Device.Serial, "setprop", "debug.mono.profile", $"127.0.0.1:{configuration.ProfilerPort},suspend");
        
        var routerProcess = DSRouter.ServerToServer(configuration.ProfilerPort+1, this);
        Thread.Sleep(1000); // wait for router to start
        var traceProcess = Trace.Collect(routerProcess.Id, resultFilePath, configuration.ProfilerMode, this);
        Thread.Sleep(1000); // wait for trace to start

        if (configuration.UninstallApp)
            DeviceBridge.Uninstall(configuration.Device.Serial, applicationId, this);
        DeviceBridge.Install(configuration.Device.Serial, configuration.OutputAssembly, this);
        DeviceBridge.Launch(configuration.Device.Serial, applicationId, this);

        disposables.Add(() => traceProcess.Terminate());
        disposables.Add(() => routerProcess.Terminate());
        disposables.Add(() => DeviceBridge.Shell(configuration.Device.Serial, "am", "force-stop", applicationId));
        disposables.Add(() => DeviceBridge.RemoveReverse(configuration.Device.Serial));
    }

    private void ProfileWindows(LaunchConfiguration configuration) {
        if (configuration.IsGCDumpProfiling)
            throw new NotSupportedException("GCDump profiling is not supported on Windows");

        var applicationName = Path.GetFileNameWithoutExtension(configuration.OutputAssembly);
        var resultFilePath = Path.Combine(configuration.TempDirectoryPath, $"{applicationName}.nettrace");

        var exeProcess = new ProcessRunner(new FileInfo(configuration.OutputAssembly), null, this).Start();
        var traceProcess = Trace.Collect(exeProcess.Id, resultFilePath, configuration.ProfilerMode, this);

        disposables.Add(() => traceProcess.Terminate());
        disposables.Add(() => exeProcess.Terminate());
    }
}
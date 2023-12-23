using System;
using System.IO;
using DotNet.Meteor.Shared;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Debug.Sdk;
using DotNet.Meteor.Debug.Sdk.Profiling;
using DotNet.Meteor.Debug.Extensions;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;

namespace DotNet.Meteor.Debug;

public partial class DebugSession {
    private void ProfileApplication(LaunchConfiguration configuration) {
        var nettracePath = Path.Combine(configuration.TempDirectoryPath, $"{configuration.GetApplicationName()}.nettrace");
        var diagnosticPort = Path.Combine(RuntimeSystem.HomeDirectory, $"{configuration.Device.Platform}-port.lock");
        ServerExtensions.TryDeleteFile(diagnosticPort);
    
        DoSafe(() => {
            if (configuration.Device.IsAndroid)
                ProfileAndroid(configuration, diagnosticPort, nettracePath);
            if (configuration.Device.IsIPhone)
                ProfileAppleMobile(configuration, diagnosticPort, nettracePath);
            if (configuration.Device.IsMacCatalyst)
                ProfileMacCatalyst(configuration, diagnosticPort, nettracePath);
            if (configuration.Device.IsWindows)
                ProfileWindows(configuration, diagnosticPort, nettracePath);
        });

        disposables.Add(() => ServerExtensions.TryDeleteFile(diagnosticPort));
        disposables.Add(() => Protocol.SendEvent(new TerminatedEvent()));
    }

    private void ProfileAppleMobile(LaunchConfiguration configuration, string diagnosticPort, string nettracePath) {
        if (configuration.Device.IsEmulator) {
            var routerProcess = DSRouter.ClientToServer(diagnosticPort, $"127.0.0.1:{configuration.ProfilerPort}", this);
            var simProcess = MonoLaunch.ProfileSim(configuration.Device.Serial, configuration.OutputAssembly, configuration.ProfilerPort, new CatchStartLogger(this, () => {
                var traceProcess = Trace.Collect(diagnosticPort, nettracePath, configuration.ProfilerMode, this);
                disposables.Insert(0, () => traceProcess.Terminate());
            }));
           
            disposables.Add(() => routerProcess.Terminate());
            disposables.Add(() => simProcess.Terminate());
        } else {
            var routerProcess = DSRouter.ServerToClient(diagnosticPort, $"127.0.0.1:{configuration.ProfilerPort}", forwardApple: true, this);
            MonoLaunch.InstallDev(configuration.Device.Serial, configuration.OutputAssembly, this);
            var devProcess = MonoLaunch.ProfileDev(configuration.Device.Serial, configuration.OutputAssembly, configuration.ProfilerPort, new CatchStartLogger(this, () => {
                var traceProcess = Trace.Collect($"{diagnosticPort},connect", nettracePath, configuration.ProfilerMode, this);
                disposables.Insert(0, () => traceProcess.Terminate());
            }));
    
            disposables.Add(() => routerProcess.Terminate());
            disposables.Add(() => devProcess.Terminate());
        }
    }
    private void ProfileMacCatalyst(LaunchConfiguration configuration, string diagnosticPort, string nettracePath) {
        var tool = AppleSdk.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder().AppendQuoted(configuration.OutputAssembly));
        processRunner.SetEnvironmentVariable("DOTNET_DiagnosticPorts", $"{diagnosticPort},suspend");
        
        var traceProcess = Trace.Collect(diagnosticPort, nettracePath, configuration.ProfilerMode, this);
        var appLaunchResult = processRunner.WaitForExit();

        disposables.Add(() => traceProcess.Terminate());

        if (!appLaunchResult.Success)
            throw new Exception(string.Join(Environment.NewLine, appLaunchResult.StandardError));
    }
    private void ProfileAndroid(LaunchConfiguration configuration, string diagnosticPort, string nettracePath) {
        var applicationId = configuration.GetApplicationName();
        if (configuration.Device.IsEmulator)
            configuration.Device.Serial = AndroidEmulator.Run(configuration.Device.Name).Serial;

        DeviceBridge.Reverse(configuration.Device.Serial, configuration.ProfilerPort, configuration.ProfilerPort+1);
        DeviceBridge.Shell(configuration.Device.Serial, "setprop", "debug.mono.profile", $"127.0.0.1:{configuration.ProfilerPort},suspend");
        
        var routerProcess = DSRouter.ServerToServer(configuration.ProfilerPort+1, this);
        System.Threading.Thread.Sleep(1000); // wait for router to start
        var traceProcess = Trace.Collect(routerProcess.Id, nettracePath, configuration.ProfilerMode, this);
        System.Threading.Thread.Sleep(1000); // wait for trace to start

        if (configuration.UninstallApp)
            DeviceBridge.Uninstall(configuration.Device.Serial, applicationId, this);
        DeviceBridge.Install(configuration.Device.Serial, configuration.OutputAssembly, this);
        DeviceBridge.Launch(configuration.Device.Serial, applicationId, this);

        disposables.Add(() => traceProcess.Terminate());
        disposables.Add(() => routerProcess.Terminate());
        disposables.Add(() => DeviceBridge.Shell(configuration.Device.Serial, "am", "force-stop", applicationId));
        disposables.Add(() => DeviceBridge.RemoveReverse(configuration.Device.Serial));
    }
    private void ProfileWindows(LaunchConfiguration configuration, string diagnosticPort, string nettracePath) {
        if (configuration.IsGCDumpProfiling)
            throw new NotSupportedException("GCDump profiling is not supported on Windows");

        var exeProcess = new ProcessRunner(new FileInfo(configuration.OutputAssembly), null, this).Start();
        var traceProcess = Trace.Collect(exeProcess.Id, nettracePath, configuration.ProfilerMode, this);

        disposables.Add(() => traceProcess.Terminate());
        disposables.Add(() => exeProcess.Terminate());
    }
}
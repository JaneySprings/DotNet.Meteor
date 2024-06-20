using System;
using System.Collections.Generic;
using System.IO;
using DotNet.Meteor.Debug.Extensions;
using DotNet.Meteor.Debug.Sdk;
using DotNet.Meteor.Debug.Sdk.Profiling;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Common;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Mono.Debugging.Soft;

namespace DotNet.Meteor.Debug;

public class GCDumpLaunchAgent : BaseLaunchAgent {
    private string diagnosticPort;
    private string gcdumpPath;
    private int applicationPID;

    public GCDumpLaunchAgent(LaunchConfiguration configuration) : base(configuration) { }
    public override void Connect(SoftDebuggerSession session) { }
    public override void Launch(IProcessLogger logger) {
        gcdumpPath = Path.Combine(Configuration.TempDirectoryPath, $"{Configuration.GetApplicationName()}.gcdump");
        diagnosticPort = Path.Combine(RuntimeSystem.HomeDirectory, $"{Configuration.Device.Platform}-port.lock");
        ServerExtensions.TryDeleteFile(diagnosticPort);

        if (Configuration.Device.IsAndroid)
            LaunchAndroid(logger);
        if (Configuration.Device.IsIPhone)
            LaunchAppleMobile(logger);
        if (Configuration.Device.IsMacCatalyst)
            LaunchMacCatalyst(logger);
        if (Configuration.Device.IsWindows)
            LaunchWindows(logger);

        Disposables.Add(() => ServerExtensions.TryDeleteFile(diagnosticPort));
    }
    public override void HandleCommand(string command, IProcessLogger logger) {
        if (!command.Equals($"{CommandPrefix}dump", System.StringComparison.OrdinalIgnoreCase))
            return;
        if (string.IsNullOrEmpty(diagnosticPort) || string.IsNullOrEmpty(gcdumpPath))
            return;

        var gcdumpProcess = applicationPID == 0
            ? GCDump.Collect($"{diagnosticPort},connect", gcdumpPath, logger)
            : GCDump.Collect(applicationPID, gcdumpPath, logger);

        Disposables.Insert(0, () => gcdumpProcess.Terminate());
    }
    public override List<CompletionItem> GetCompletionItems() {
        return new List<CompletionItem>() {
            new CompletionItem() { Label = "dump", Type = CompletionItemType.Snippet }
        };
    }

    private void LaunchAppleMobile(IProcessLogger logger) {
        if (Configuration.Device.IsEmulator) {
            var routerProcess = DSRouter.ServerToClient($"{diagnosticPort}", $"127.0.0.1:{Configuration.ProfilerPort}", false, logger);
            var simProcess = MonoLaunch.ProfileSim(Configuration.Device.Serial, Configuration.OutputAssembly, $"127.0.0.1:{Configuration.ProfilerPort},nosuspend,listen", logger);

            Disposables.Add(() => routerProcess.Terminate());
            Disposables.Add(() => simProcess.Terminate());
        } else {
            var routerProcess = DSRouter.ServerToClient(diagnosticPort, $"127.0.0.1:{Configuration.ProfilerPort}", forwardApple: true, logger);
            MonoLaunch.InstallDev(Configuration.Device.Serial, Configuration.OutputAssembly, logger);
            var devProcess = MonoLaunch.ProfileDev(Configuration.Device.Serial, Configuration.OutputAssembly, $"127.0.0.1:{Configuration.ProfilerPort},nosuspend,listen", logger);

            Disposables.Add(() => routerProcess.Terminate());
            Disposables.Add(() => devProcess.Terminate());
        }
    }
    private void LaunchMacCatalyst(IProcessLogger logger) {
        var tool = AppleSdk.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder().AppendQuoted(Configuration.OutputAssembly));

        diagnosticPort = $"127.0.0.1:{Configuration.ProfilerPort}";
        processRunner.SetEnvironmentVariable("DOTNET_DiagnosticPorts", $"127.0.0.1:{Configuration.ProfilerPort},nosuspend,listen");

        var appLaunchResult = processRunner.WaitForExit();
        if (!appLaunchResult.Success)
            throw new Exception(string.Join(Environment.NewLine, appLaunchResult.StandardError));
    }
    private void LaunchAndroid(IProcessLogger logger) {
        var applicationId = Configuration.GetApplicationName();
        if (Configuration.Device.IsEmulator)
            Configuration.Device.Serial = AndroidEmulator.Run(Configuration.Device.Name).Serial;

        DeviceBridge.Reverse(Configuration.Device.Serial, Configuration.ProfilerPort, Configuration.ProfilerPort + 1);
        DeviceBridge.Shell(Configuration.Device.Serial, "setprop", "debug.mono.profile", $"127.0.0.1:{Configuration.ProfilerPort},nosuspend,connect");

        var routerProcess = DSRouter.ServerToServer(Configuration.ProfilerPort + 1, logger);
        applicationPID = routerProcess.Id;

        Disposables.Add(() => routerProcess.Terminate());
        Disposables.Add(() => DeviceBridge.RemoveReverse(Configuration.Device.Serial));

        if (Configuration.UninstallApp)
            DeviceBridge.Uninstall(Configuration.Device.Serial, applicationId, logger);
        DeviceBridge.Install(Configuration.Device.Serial, Configuration.OutputAssembly, logger);
        DeviceBridge.Launch(Configuration.Device.Serial, applicationId, logger);

        Disposables.Add(() => DeviceBridge.Shell(Configuration.Device.Serial, "am", "force-stop", applicationId));
    }
    private void LaunchWindows(IProcessLogger logger) {
        var exeProcess = new ProcessRunner(new FileInfo(Configuration.OutputAssembly), null, logger).Start();
        applicationPID = exeProcess.Id;

        Disposables.Add(() => exeProcess.Terminate());
    }
}
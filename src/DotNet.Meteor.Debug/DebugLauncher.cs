﻿using System;
using System.IO;
using DotNet.Meteor.Shared;
using DotNet.Meteor.Processes;
using System.Net;
using Mono.Debugging.Soft;
using DotNet.Meteor.Debug.Sdb;
using DotNet.Meteor.Debug.Sdk;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;

namespace DotNet.Meteor.Debug;

public partial class DebugSession {
    private void Connect(LaunchConfiguration configuration) {
        SoftDebuggerStartArgs arguments = null;

        if (configuration.Device.IsAndroid || (configuration.Device.IsIPhone && !configuration.Device.IsEmulator))
            arguments = new ClientConnectionProvider(IPAddress.Loopback, configuration.DebugPort, configuration.Project.Name);
        else if (configuration.Device.IsIPhone || configuration.Device.IsMacCatalyst)
            arguments = new ServerConnectionProvider(IPAddress.Loopback, configuration.DebugPort, configuration.Project.Name);

        if (arguments == null || !configuration.IsDebugConfiguration)
            throw new ProtocolException("Unable to connect to the debugger");

        session.Run(new SoftDebuggerStartInfo(arguments), configuration.DebuggerSessionOptions);
        OnOutputDataReceived("Debugger is ready and listening...");
    }

    private void LaunchApplication(LaunchConfiguration configuration) {
        DoSafe(() => {
            if (configuration.Device.IsAndroid)
                LaunchAndroid(configuration);

            if (configuration.Device.IsIPhone)
                LaunchApple(configuration);

            if (configuration.Device.IsMacCatalyst)
                LaunchMacCatalyst(configuration);

            if (configuration.Device.IsWindows)
                LaunchWindows(configuration);
        });
    }

    private void LaunchApple(LaunchConfiguration configuration) {
        // TODO: Implement Apple launching for Windows
        // if (RuntimeSystem.IsWindows) {
        //     IDeviceTool.Installer(configuration.Device.Serial, configuration.OutputAssembly, this);
            
        //     var debugProcess = IDeviceTool.Debug(configuration.Device.Serial, configuration.GetApplicationId(), configuration.DebugPort, this);
        //     disposables.Add(() => debugProcess.Kill());
        //     return;
        // }

        if (configuration.Device.IsEmulator) {
            var debugProcess = MonoLaunch.DebugSim(configuration.Device.Serial, configuration.OutputAssembly, configuration.DebugPort, this);
            disposables.Add(() => debugProcess.Kill());
        } else {
            var forwardingProcess = MonoLaunch.TcpTunnel(configuration.Device.Serial, configuration.DebugPort, this);
            MonoLaunch.InstallDev(configuration.Device.Serial, configuration.OutputAssembly, this);
            
            var debugProcess = MonoLaunch.DebugDev(configuration.Device.Serial, configuration.OutputAssembly, configuration.DebugPort, this);
            disposables.Add(() => debugProcess.Kill());
            disposables.Add(() => forwardingProcess.Kill());
        }
    }

    private void LaunchMacCatalyst(LaunchConfiguration configuration) {
        var tool = AppleUtilities.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder().AppendQuoted(configuration.OutputAssembly));
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_HOSTS__", "127.0.0.1");
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_PORT__", configuration.DebugPort.ToString());
        var result = processRunner.WaitForExit();

        if (!result.Success)
            throw new ProtocolException(string.Join(Environment.NewLine, result.StandardError));
    }

    private void LaunchWindows(LaunchConfiguration configuration) {
        var program = new FileInfo(configuration.OutputAssembly);
        var process = new ProcessRunner(program, new ProcessArgumentBuilder(), this).Start();
        disposables.Add(() => process.Kill());
    }

    private void LaunchAndroid(LaunchConfiguration configuration) {
        var applicationId = configuration.GetApplicationId();
        if (configuration.Device.IsEmulator)
            configuration.Device.Serial = AndroidEmulator.Run(configuration.Device.Name).Serial;

        if (configuration.ReloadHostPort > 0)
            DeviceBridge.Forward(configuration.Device.Serial, configuration.ReloadHostPort);

        DeviceBridge.Forward(configuration.Device.Serial, configuration.DebugPort);

        if (configuration.UninstallApp)
            DeviceBridge.Uninstall(configuration.Device.Serial, applicationId, this);

        DeviceBridge.Install(configuration.Device.Serial, configuration.OutputAssembly, this);
        DeviceBridge.Shell(configuration.Device.Serial, "setprop", "debug.mono.connect", $"port={configuration.DebugPort}");
        DeviceBridge.Launch(configuration.Device.Serial, applicationId, this);
        DeviceBridge.Flush(configuration.Device.Serial);

        var logcatFirstChannelProcess = DeviceBridge.Logcat(configuration.Device.Serial, "system,crash", "*:I", this);
        var logcatSecondChannelProcess = DeviceBridge.Logcat(configuration.Device.Serial, "main", "DOTNET:I", this);

        disposables.Add(() => logcatFirstChannelProcess.Kill());
        disposables.Add(() => logcatSecondChannelProcess.Kill());
        disposables.Add(() => DeviceBridge.RemoveForward(configuration.Device.Serial));
    }
}
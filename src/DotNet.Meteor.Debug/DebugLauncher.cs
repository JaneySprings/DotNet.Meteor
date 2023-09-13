using System;
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
    private void Connect(LaunchConfiguration options, int port) {
        SoftDebuggerStartArgs arguments = null;

        if (options.Device.IsAndroid || (options.Device.IsIPhone && !options.Device.IsEmulator))
            arguments = new ClientConnectionProvider(IPAddress.Loopback, port, options.Project.Name);
        else if (options.Device.IsIPhone || options.Device.IsMacCatalyst)
            arguments = new ServerConnectionProvider(IPAddress.Loopback, port, options.Project.Name);

        if (arguments == null || !options.IsDebug)
            return;

        session.Run(new SoftDebuggerStartInfo(arguments), sessionOptions);
        OnOutputDataReceived("Debugger is ready and listening...");
    }

    private void LaunchApplication(LaunchConfiguration configuration, int port) {
        if (configuration.Device.IsAndroid)
            LaunchAndroid(configuration, port);
        if (configuration.Device.IsIPhone)
            LaunchApple(configuration, port);
        if (configuration.Device.IsMacCatalyst)
            LaunchMacCatalyst(configuration, port);
        if (configuration.Device.IsWindows)
            LaunchWindows(configuration);
    }

    private void LaunchApple(LaunchConfiguration configuration, int port) {
        if (RuntimeSystem.IsWindows) {
            IDeviceTool.Installer(configuration.Device.Serial, configuration.OutputAssembly, this);
            
            var debugProcess = IDeviceTool.Debug(configuration.Device.Serial, configuration.GetApplicationId(), port, this);
            disposables.Add(() => debugProcess.Kill());
            return;
        }

        if (configuration.Device.IsEmulator) {
            var debugProcess = MonoLaunch.DebugSim(configuration.Device.Serial, configuration.OutputAssembly, port, this);
            disposables.Add(() => debugProcess.Kill());
        } else {
            var forwardingProcess = MonoLaunch.TcpTunnel(configuration.Device.Serial, port, this);
            MonoLaunch.InstallDev(configuration.Device.Serial, configuration.OutputAssembly, this);
            
            var debugProcess = MonoLaunch.DebugDev(configuration.Device.Serial, configuration.OutputAssembly, port, this);
            disposables.Add(() => debugProcess.Kill());
            disposables.Add(() => forwardingProcess.Kill());
        }
    }

    private void LaunchMacCatalyst(LaunchConfiguration configuration, int port) {
        var tool = AppleUtilities.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder()
            .AppendQuoted(configuration.OutputAssembly)
        );
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_HOSTS__", "127.0.0.1");
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_PORT__", port.ToString());
        var result = processRunner.WaitForExit();

        if (!result.Success)
            throw new ProtocolException(string.Join(Environment.NewLine, result.StandardError));
    }

    private void LaunchWindows(LaunchConfiguration configuration) {
        var program = new FileInfo(configuration.OutputAssembly);
        var process = new ProcessRunner(program, new ProcessArgumentBuilder(), this).Start();
        disposables.Add(() => process.Kill());
    }

    private void LaunchAndroid(LaunchConfiguration configuration, int port) {
        var applicationId = configuration.GetApplicationId();
        if (configuration.Device.IsEmulator)
            configuration.Device.Serial = AndroidEmulator.Run(configuration.Device.Name).Serial;

        if (configuration.ReloadHostPort > 0)
            DeviceBridge.Forward(configuration.Device.Serial, configuration.ReloadHostPort);

        DeviceBridge.Forward(configuration.Device.Serial, port);

        if (configuration.UninstallApp)
            DeviceBridge.Uninstall(configuration.Device.Serial, applicationId, this);

        DeviceBridge.Install(configuration.Device.Serial, configuration.OutputAssembly, this);
        DeviceBridge.Shell(configuration.Device.Serial, "setprop", "debug.mono.connect", $"port={port}");
        DeviceBridge.Launch(configuration.Device.Serial, applicationId, this);
        DeviceBridge.Flush(configuration.Device.Serial);

        var logcatFirstChannelProcess = DeviceBridge.Logcat(configuration.Device.Serial, "system,crash", "*:I", this);
        var logcatSecondChannelProcess = DeviceBridge.Logcat(configuration.Device.Serial, "main", "DOTNET:I", this);

        disposables.Add(() => logcatFirstChannelProcess.Kill());
        disposables.Add(() => logcatSecondChannelProcess.Kill());
        disposables.Add(() => DeviceBridge.RemoveForward(configuration.Device.Serial));
    }
}
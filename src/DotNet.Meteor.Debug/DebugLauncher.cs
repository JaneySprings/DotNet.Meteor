using System;
using System.Collections.Generic;
using System.IO;
using DotNet.Meteor.Shared;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Android;
using DotNet.Meteor.Apple;
using System.Net;
using Mono.Debugging.Soft;
using DotNet.Meteor.Debug.Pipeline;
using Process = System.Diagnostics.Process;

namespace DotNet.Meteor.Debug;

public partial class DebugSession  {
    private const int MAX_CONNECTION_ATTEMPTS = 20;
    private const int CONNECTION_ATTEMPT_INTERVAL = 500;


    private void Connect(LaunchData options, int port) {
        lock (this.locker) {
            SoftDebuggerStartArgs arguments = null;

            if (!options.IsDebug)
                return;

            if (options.Device.IsAndroid) {
                arguments = new SoftDebuggerConnectArgs(options.Project.Name, IPAddress.Loopback, port) {
                    MaxConnectionAttempts = MAX_CONNECTION_ATTEMPTS,
                    TimeBetweenConnectionAttempts = CONNECTION_ATTEMPT_INTERVAL
                };
            }
            if (options.Device.IsIPhone || options.Device.IsMacCatalyst) {
                arguments = new StreamCommandConnectionDebuggerArgs(options.Project.Name, IPAddress.Loopback, port) {
                    MaxConnectionAttempts = MAX_CONNECTION_ATTEMPTS
                };
            }

            if (arguments == null)
                return;

            this.debuggerExecuting = true;
            this.session.Run(new SoftDebuggerStartInfo(arguments), this.sessionOptions);
            OnOutputDataReceived("Debugger is ready and listening...");
        }
    }

    private void LaunchApplication(LaunchData configuration, int port, List<Process> processes) {
        if (configuration.Device.IsAndroid)
            LaunchAndroid(configuration, port, processes);
        if (configuration.Device.IsIPhone)
            LaunchApple(configuration, port, processes);
        if (configuration.Device.IsMacCatalyst)
            LaunchMacCatalyst(configuration, port);
        if (configuration.Device.IsWindows)
            LaunchWindows(configuration, processes);
    }

    private void LaunchApple(LaunchData configuration, int port, List<Process> processes) {
        if (RuntimeSystem.IsWindows) {
            IDeviceTool.Installer(configuration.Device.Serial, configuration.OutputAssembly, this);
            processes.Add(IDeviceTool.Debug(configuration.Device.Serial, configuration.GetApplicationId(), port, this));
            return;
        }

        if (configuration.Device.IsEmulator) {
            processes.Add(MonoLaunch.DebugSim(configuration.Device.Serial, configuration.OutputAssembly, port, this));
        } else {
            //processes.Add(MLaunch.TcpTunnel(configuration.Device.Serial, port));
            MonoLaunch.InstallDev(configuration.Device.Serial, configuration.OutputAssembly, this);
            processes.Add(MonoLaunch.DebugDev(configuration.Device.Serial, configuration.OutputAssembly, port, this));
        }
    }

    private void LaunchMacCatalyst(LaunchData configuration, int port) {
        var tool = DotNet.Meteor.Apple.PathUtils.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder()
            .AppendQuoted(configuration.OutputAssembly)
        );
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_HOSTS__", "127.0.0.1");
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_PORT__", port.ToString());
        var result = processRunner.WaitForExit();

        if (result.ExitCode != 0)
            throw new Exception(string.Join(Environment.NewLine, result.StandardError));
    }

    private void LaunchWindows(LaunchData configuration, List<Process> processes) {
        var program = new FileInfo(configuration.OutputAssembly);
        var process = new ProcessRunner(program, new ProcessArgumentBuilder(), this)
            .Start();
        processes.Add(process);
    }

    private void LaunchAndroid(LaunchData configuration, int port, List<Process> processes) {
        var applicationId = configuration.GetApplicationId();

        if (configuration.Device.IsEmulator)
            configuration.Device.Serial = Emulator.Run(configuration.Device.Name).Serial;

        DeviceBridge.Uninstall(configuration.Device.Serial, applicationId, this);
        DeviceBridge.Install(configuration.Device.Serial, configuration.OutputAssembly, this);

        if (configuration.IsDebug) {
            var androidSdk = DotNet.Meteor.Android.PathUtils.SdkLocation();
            var arguments = new ProcessArgumentBuilder()
                .Append("build").AppendQuoted(configuration.Project.Path)
                .Append( "-t:_Run")
                .Append($"-f:{configuration.Framework}")
                .Append($"-p:AndroidSdkDirectory=\"{androidSdk}\"")
                .Append($"-p:AdbTarget=-s%20{configuration.Device.Serial}")
                .Append( "-p:AndroidAttachDebugger=true")
                .Append($"-p:AndroidSdbTargetPort={port}")
                .Append($"-p:AndroidSdbHostPort={port}");

            var result = new ProcessRunner(Shared.PathUtils.DotNetTool(), arguments, this)
                .WaitForExit();

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));
        } else {
            DeviceBridge.Launch(configuration.Device.Serial, applicationId, this);
        }

        var logger = DeviceBridge.Logcat(configuration.Device.Serial, this);
        processes.Add(logger);
    }
}
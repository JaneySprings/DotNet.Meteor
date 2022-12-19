﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using DotNet.Mobile.Shared;
using Microsoft.Sdk;
using Android.Sdk;
using Apple.Sdk;

namespace DotNet.Mobile.Debug.Session;

public partial class MonoDebugSession {
    protected void LaunchApplication(LaunchData configuration, int port, List<Process> processes) {
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
        if (!configuration.Device.IsEmulator) {
            MLaunch.InstallOnDevice(configuration.ExecutablePath, configuration.Device.Serial, this);
            processes.Add(MLaunch.TcpTunnel(configuration.Device.Serial, port));
            processes.Add(MLaunch.LaunchOnDevice(configuration.ExecutablePath, configuration.Device.Serial, port, this));
        } else {
            processes.Add(MLaunch.LaunchOnSimulator(configuration.ExecutablePath, configuration.Device.Serial, port, this));
        }
    }

    private void LaunchMacCatalyst(LaunchData configuration, int port) {
        var tool = Apple.Sdk.PathUtils.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder()
            .AppendQuoted(configuration.ExecutablePath)
        );
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_HOSTS__", "127.0.0.1");
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_PORT__", port.ToString());
        var result = processRunner.WaitForExit();

        if (result.ExitCode != 0)
            throw new Exception(string.Join(Environment.NewLine, result.StandardError));
    }

    private void LaunchWindows(LaunchData configuration, List<Process> processes) {
        var program = new FileInfo(configuration.ExecutablePath);
        var process = new ProcessRunner(program, new ProcessArgumentBuilder(), this)
            .Start();
        processes.Add(process);
    }

    private void LaunchAndroid(LaunchData configuration, int port, List<Process> processes) {
        if (configuration.Device.IsEmulator)
            configuration.Device.Serial = Emulator.Run(configuration.Device.Name);

        DeviceBridge.Uninstall(configuration.Device.Serial, configuration.AppId, this);
        DeviceBridge.Install(configuration.Device.Serial, configuration.ExecutablePath, this);

        if (configuration.IsDebug) {
            var androidSdk = Android.Sdk.PathUtils.SdkLocation();
            var arguments = new ProcessArgumentBuilder()
                .Append("build").AppendQuoted(configuration.Project.Path)
                .Append( "-t:_Run")
                .Append($"-f:{configuration.Framework}")
                .Append($"-p:AndroidSdkDirectory=\"{androidSdk}\"")
                .Append($"-p:AdbTarget=-s%20{configuration.Device.Serial}")
                .Append( "-p:AndroidAttachDebugger=true")
                .Append($"-p:AndroidSdbTargetPort={port}")
                .Append($"-p:AndroidSdbHostPort={port}");
            DotNetTool.Execute(arguments, this);
        } else {
            DeviceBridge.Launch(configuration.Device.Serial, configuration.AppId, this);
        }

        var logger = DeviceBridge.Logcat(configuration.Device.Serial, this);
        processes.Add(logger);
    }
}
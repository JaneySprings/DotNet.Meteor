﻿using System;
using System.Collections.Generic;
using System.IO;
using DotNet.Mobile.Debug.Protocol;
using System.Diagnostics;
using DotNet.Mobile.Shared;
using Microsoft.Sdk;
using Android.Sdk;
using Apple.Sdk;

namespace DotNet.Mobile.Debug.Session;

public abstract class DebugSession : Session {
    protected bool clientLinesStartAt1 = true;
    protected bool clientPathsAreURI = true;

    private readonly Dictionary<string, Action<Response, Argument>> requestHandlers;

    protected int ConvertDebuggerLineToClient(int line) => this.clientLinesStartAt1 ? line : line - 1;
    protected int ConvertClientLineToDebugger(int line) => this.clientLinesStartAt1 ? line : line + 1;


    protected DebugSession() {
        requestHandlers = new Dictionary<string, Action<Response, Argument>>() {
            { "initialize", Initialize },
            { "launch", Launch },
            { "attach", Attach },
            { "next", Next },
            { "continue", Continue },
            { "stepIn", StepIn },
            { "stepOut", StepOut },
            { "pause", Pause },
            { "stackTrace", StackTrace },
            { "scopes", Scopes },
            { "variables", Variables },
            { "source", Source },
            { "threads", Threads },
            { "setBreakpoints", SetBreakpoints },
            { "setFunctionBreakpoints", SetFunctionBreakpoints },
            { "setExceptionBreakpoints", SetExceptionBreakpoints },
            { "evaluate", Evaluate },
            { "disconnect", Disconnect }
        };
    }

    protected override void DispatchRequest(string command, Argument args, Response response) {
        try {
            if (requestHandlers.TryGetValue(command, out var handler)) {
                handler.Invoke(response, args);
            } else {
                SendErrorResponse(response, 1014, $"unrecognized request '{command}'");
            }
        } catch (Exception e) {
            SendErrorResponse(response, 1104, $"error while processing request '{command}' (exception: {e.Message} -> {e.StackTrace})");
        }
    }

    public abstract void Initialize(Response response, Argument args);
    public abstract void Launch(Response response, Argument arguments);
    public abstract void Attach(Response response, Argument arguments);
    public abstract void Disconnect(Response response, Argument arguments);
    public abstract void SetFunctionBreakpoints(Response response, Argument arguments);
    public abstract void SetExceptionBreakpoints(Response response, Argument arguments);
    public abstract void SetBreakpoints(Response response, Argument arguments);
    public abstract void Continue(Response response, Argument arguments);
    public abstract void Next(Response response, Argument arguments);
    public abstract void StepIn(Response response, Argument arguments);
    public abstract void StepOut(Response response, Argument arguments);
    public abstract void Pause(Response response, Argument arguments);
    public abstract void StackTrace(Response response, Argument arguments);
    public abstract void Scopes(Response response, Argument arguments);
    public abstract void Variables(Response response, Argument arguments);
    public abstract void Source(Response response, Argument arguments);
    public abstract void Threads(Response response, Argument arguments);
    public abstract void Evaluate(Response response, Argument arguments);


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
            MLaunch.InstallOnDevice(configuration.BundlePath, configuration.Device.Serial, this);
            processes.Add(MLaunch.TcpTunnel(configuration.Device.Serial, port));
            processes.Add(MLaunch.LaunchOnDevice(configuration.BundlePath, configuration.Device.Serial, port, this));
        } else {
            processes.Add(MLaunch.LaunchOnSimulator(configuration.BundlePath, configuration.Device.Serial, port, this));
        }
    }

    private void LaunchMacCatalyst(LaunchData configuration, int port) {
        var tool = Apple.Sdk.PathUtils.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder()
            .AppendQuoted(configuration.BundlePath)
        );
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_HOSTS__", "127.0.0.1");
        processRunner.SetEnvironmentVariable("__XAMARIN_DEBUG_PORT__", port.ToString());
        var result = processRunner.WaitForExit();

        if (result.ExitCode != 0)
            throw new Exception(string.Join(Environment.NewLine, result.StandardError));
    }

    private void LaunchWindows(LaunchData configuration, List<Process> processes) {
        var program = new FileInfo(configuration.BundlePath);
        var process = new ProcessRunner(program, new ProcessArgumentBuilder(), this)
            .Start();
        processes.Add(process);
    }

    private void LaunchAndroid(LaunchData configuration, int port, List<Process> processes) {
        if (configuration.Device.IsEmulator)
            configuration.Device.Serial = Emulator.Run(configuration.Device.Name);

        DeviceBridge.Uninstall(configuration.Device.Serial, configuration.AppId, this);
        DeviceBridge.Install(configuration.Device.Serial, configuration.BundlePath, this);

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
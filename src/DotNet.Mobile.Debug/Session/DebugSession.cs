using System;
using System.Collections.Generic;
using DotNet.Mobile.Debug.Protocol;
using System.Diagnostics;
using DotNet.Mobile.Shared;
using Microsoft.Sdk;
using Android.Sdk;
using XCode.Sdk;

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
        if (configuration.Platform == Platform.Android) {
            if (configuration.Device.IsEmulator && !configuration.Device.IsRunning)
                configuration.Device.Serial = Emulator.Run(configuration.Device.Name);

            var androidSdk = Android.Sdk.PathUtils.SdkLocation();
            DotNetRunner.Execute(new ProcessArgumentBuilder()
                .Append("build", $"\"{configuration.Project.Path}\"")
                .Append( "-t:_Upload;_Run")
                .Append($"-f:{configuration.Framework}")
                .Append( "-p:AndroidAttachDebugger=true")
                .Append($"-p:AdbTarget=-s%20{configuration.Device.Serial}")
                .Append($"-p:AndroidSdbTargetPort={port}")
                .Append($"-p:AndroidSdbHostPort={port}")
                .Append($"-p:AndroidSdkDirectory=\"{androidSdk}\""), this);

            var logger = DeviceBridge.Logcat(configuration.Device.Serial, this);
            processes.Add(logger);
        }

        if (configuration.Platform == Platform.iOS) {
            if (!configuration.Device.IsEmulator) {
                var tunnel = MLaunch.TcpTunnel(configuration.Device, port);
                processes.Add(tunnel);
            }
            var deviceProccess = MLaunch.LaunchAppForDebug(configuration.BundlePath, configuration.Device, port, this);
            processes.Add(deviceProccess);
        }
    }
}
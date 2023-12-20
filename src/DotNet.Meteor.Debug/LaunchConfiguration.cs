﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNet.Meteor.Shared;
using DotNet.Meteor.Debug.Extensions;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Newtonsoft.Json.Linq;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug;

public class LaunchConfiguration {
    public DebuggerSessionOptions DebuggerSessionOptions { get; init; }
    public DeviceData Device { get; init; }
    public Project Project { get; init; }

    public string OutputAssembly { get; init; }
    public string Framework { get; init; }
    public string Target { get; init; }
    public bool UninstallApp { get; init; }

    public int DebugPort { get; set; }
    public int ReloadHostPort { get; init; }
    public int ProfilerPort { get; init; }
    public string ProfilerMode { get; init; }

    public bool IsDebugConfiguration => Target.Equals("debug", StringComparison.OrdinalIgnoreCase);
    public bool IsProfileConfiguration => !string.IsNullOrEmpty(ProfilerMode);
    public string TempDirectoryPath => Path.Combine(Path.GetDirectoryName(Project.Path), ".meteor");

    public LaunchConfiguration(Dictionary<string, JToken> configurationProperties) {
        Project = configurationProperties["selectedProject"].ToObject<Project>(TrimmableContext.Default.Project);
        Device = configurationProperties["selectedDevice"].ToObject<DeviceData>(TrimmableContext.Default.DeviceData);
        Target = configurationProperties["selectedTarget"].ToObject<string>(TrimmableContext.Default.String);
        ReloadHostPort = configurationProperties["reloadHost"].ToObject<int>(TrimmableContext.Default.Int32);
        ProfilerPort = configurationProperties["profilerPort"].ToObject<int>(TrimmableContext.Default.Int32);
        UninstallApp = configurationProperties["uninstallApp"].ToObject<bool>(TrimmableContext.Default.Boolean);
        DebugPort = configurationProperties["debuggingPort"].ToObject<int>(TrimmableContext.Default.Int32);
        
        if (configurationProperties.TryGetValue("profilerMode", out var profilerModeToken))
            ProfilerMode = profilerModeToken.ToObject<string>(TrimmableContext.Default.String);
        
        DebuggerSessionOptions = GetDebuggerSessionOptions(configurationProperties["debuggerOptions"]);
        Framework = Project.Frameworks.First(it => it.ContainsInsensitive(Device.Platform));
        OutputAssembly = Project.FindOutputApplication(Target, Framework, Device, message => {
            throw new ProtocolException($"Failed to load launch configuration. {message}");
        });
    }

    public string GetApplicationId() {
        if (!Device.IsAndroid)
            throw new ProtocolException("Application ID not implemented.");

        var assemblyName = Path.GetFileNameWithoutExtension(OutputAssembly);
        return assemblyName.Replace("-Signed", "");
    }

    private DebuggerSessionOptions GetDebuggerSessionOptions(JToken debuggerJsonToken) {
        var debuggerOptions = ServerExtensions.DefaultDebuggerOptions;
        var options = debuggerJsonToken.ToObject<DebuggerOptions>(DebuggerOptionsContext.Default.DebuggerOptions);
        if (options == null)
            return debuggerOptions;

        debuggerOptions.EvaluationOptions.EvaluationTimeout = options.EvaluationTimeout;
        debuggerOptions.EvaluationOptions.MemberEvaluationTimeout = options.MemberEvaluationTimeout;
        debuggerOptions.EvaluationOptions.AllowTargetInvoke = options.AllowTargetInvoke;
        debuggerOptions.EvaluationOptions.AllowMethodEvaluation = options.AllowMethodEvaluation;
        debuggerOptions.EvaluationOptions.AllowToStringCalls = options.AllowToStringCalls;
        debuggerOptions.EvaluationOptions.FlattenHierarchy = options.FlattenHierarchy;
        debuggerOptions.EvaluationOptions.GroupPrivateMembers = options.GroupPrivateMembers;
        debuggerOptions.EvaluationOptions.GroupStaticMembers = options.GroupStaticMembers;
        debuggerOptions.EvaluationOptions.UseExternalTypeResolver = options.UseExternalTypeResolver;
        debuggerOptions.EvaluationOptions.CurrentExceptionTag = options.CurrentExceptionTag;
        debuggerOptions.EvaluationOptions.EllipsizeStrings = options.EllipsizeStrings;
        debuggerOptions.EvaluationOptions.EllipsizedLength = options.EllipsizedLength;
        debuggerOptions.EvaluationOptions.IntegerDisplayFormat = DebuggerOptions.GetIntegerDisplayFormat(options.IntegerDisplayFormat);

        return debuggerOptions;
    }
}
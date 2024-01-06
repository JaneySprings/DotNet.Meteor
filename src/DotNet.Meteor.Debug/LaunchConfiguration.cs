using System;
using System.Collections.Generic;
using System.IO;
using DotNet.Meteor.Shared;
using DotNet.Meteor.Debug.Extensions;
using Newtonsoft.Json.Linq;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug;

public class LaunchConfiguration {
    public DebuggerSessionOptions DebuggerSessionOptions { get; init; }
    public string OutputAssembly { get; init; }
    public DeviceData Device { get; init; }
    public Project Project { get; init; }
    public string Target { get; init; }
    public string ProfilerMode { get; init; }
    public bool UninstallApp { get; init; }
    public bool SkipDebug { get; init; }

    public int DebugPort { get; init; }
    public int ProfilerPort { get; init; }
    public int ReloadHostPort { get; init; }

    public string TempDirectoryPath => Path.Combine(Path.GetDirectoryName(Project.Path), ".meteor");

    public LaunchConfiguration(Dictionary<string, JToken> configurationProperties) {
        Project = configurationProperties["selectedProject"].ToObject<Project>(TrimmableContext.Default.Project);
        Device = configurationProperties["selectedDevice"].ToObject<DeviceData>(TrimmableContext.Default.DeviceData);
        Target = configurationProperties["selectedTarget"].ToObject<string>(TrimmableContext.Default.String);
        UninstallApp = configurationProperties["uninstallApp"].ToObject<bool>(TrimmableContext.Default.Boolean);
        SkipDebug = configurationProperties["skipDebug"].ToObject<bool>(TrimmableContext.Default.Boolean);

        DebugPort = configurationProperties["debuggingPort"].ToObject<int>(TrimmableContext.Default.Int32);
        ReloadHostPort = configurationProperties["reloadHost"].ToObject<int>(TrimmableContext.Default.Int32);
        ProfilerPort = configurationProperties["profilerPort"].ToObject<int>(TrimmableContext.Default.Int32);

        DebuggerSessionOptions = GetDebuggerSessionOptions(configurationProperties["debuggerOptions"]);
        OutputAssembly = Project.FindOutputApplication(Target, Device, message => {
            ServerExtensions.ThrowException(message);
            return string.Empty;
        });
        
        if (configurationProperties.TryGetValue("profilerMode", out var profilerModeToken))
            ProfilerMode = profilerModeToken.ToObject<string>(TrimmableContext.Default.String);

        DebugPort = DebugPort == 0 ? ServerExtensions.FindFreePort() : DebugPort;
        ReloadHostPort = ReloadHostPort == 0 ? ServerExtensions.FindFreePort() : ReloadHostPort;
        ProfilerPort = ProfilerPort == 0 ? ServerExtensions.FindFreePort() : ProfilerPort;
    }

    public string GetApplicationName() {
        if (!Device.IsAndroid)
            return Path.GetFileNameWithoutExtension(OutputAssembly);

        var assemblyName = Path.GetFileNameWithoutExtension(OutputAssembly);
        return assemblyName.Replace("-Signed", "");
    }
    public BaseLaunchAgent GetLauchAgent() {
        if (ProfilerMode.EqualsInsensitive("trace"))
            return new TraceLaunchAgent(this);
        if (ProfilerMode.EqualsInsensitive("gcdump"))
            return new GCDumpLaunchAgent(this);

        if (SkipDebug || Target.EqualsInsensitive("release"))
            return new NoDebugLaunchAgent(this);
        if (Target.EqualsInsensitive("debug"))
            return new DebugLaunchAgent(this);
 
        throw new NotSupportedException("Could not create launch agent for current configuration");
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
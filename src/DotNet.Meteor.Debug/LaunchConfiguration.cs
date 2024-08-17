using System;
using System.Collections.Generic;
using System.IO;
using DotNet.Meteor.Common;
using DotNet.Meteor.Debug.Extensions;
using Newtonsoft.Json.Linq;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug;

public class LaunchConfiguration {
    public DebuggerSessionOptions DebuggerSessionOptions { get; init; }
    public string OutputAssembly { get; init; }
    public DeviceData Device { get; init; }
    public Project Project { get; init; }
    public string Configuration { get; init; }
    public string ProfilerMode { get; init; }
    public bool UninstallApp { get; init; }
    public bool SkipDebug { get; init; }

    public int DebugPort { get; init; }
    public int ProfilerPort { get; init; }
    public int ReloadHostPort { get; init; }

    public string TempDirectoryPath => Path.Combine(Path.GetDirectoryName(Project.Path), ".meteor");

    public LaunchConfiguration(Dictionary<string, JToken> configurationProperties) {
        Project = configurationProperties.TryGetValue("project").ToObject(TrimmableContext.Default.Project);
        Device = configurationProperties.TryGetValue("device").ToObject(TrimmableContext.Default.DeviceData);
        Configuration = configurationProperties.TryGetValue("configuration").ToObject(TrimmableContext.Default.String);
        UninstallApp = configurationProperties.TryGetValue("uninstallApp").ToObject(TrimmableContext.Default.Boolean);
        SkipDebug = configurationProperties.TryGetValue("skipDebug").ToObject(TrimmableContext.Default.Boolean);
        OutputAssembly = configurationProperties.TryGetValue("program").ToObject(TrimmableContext.Default.String);
        ProfilerMode = configurationProperties.TryGetValue("profilerMode").ToObject(TrimmableContext.Default.String);

        DebugPort = configurationProperties.TryGetValue("debuggingPort").ToObject(TrimmableContext.Default.Int32);
        ReloadHostPort = configurationProperties.TryGetValue("reloadHost").ToObject(TrimmableContext.Default.Int32);
        ProfilerPort = configurationProperties.TryGetValue("profilerPort").ToObject(TrimmableContext.Default.Int32);
        DebuggerSessionOptions = GetDebuggerSessionOptions(configurationProperties.TryGetValue("debuggerOptions"));

        if (string.IsNullOrEmpty(OutputAssembly))
            OutputAssembly = Project.FindOutputApplication(Configuration, Device, message => throw ServerExtensions.GetProtocolException(message));

        DebugPort = DebugPort == 0 ? ServerExtensions.FindFreePort() : DebugPort;
        ReloadHostPort = ReloadHostPort == 0 ? ServerExtensions.FindFreePort() : ReloadHostPort;
        ProfilerPort = ProfilerPort == 0 ? ServerExtensions.FindFreePort() : ProfilerPort;

        if (!Directory.Exists(TempDirectoryPath))
            Directory.CreateDirectory(TempDirectoryPath);
    }

    public string GetApplicationName() {
        if (!Device.IsAndroid)
            return Path.GetFileNameWithoutExtension(OutputAssembly);

        var assemblyName = Path.GetFileNameWithoutExtension(OutputAssembly);
        return assemblyName.Replace("-Signed", "");
    }
    public string GetApplicationAssembliesDirectory() {
        if (Device.IsMacCatalyst)
            return Path.Combine(OutputAssembly, "Contents", "MonoBundle");
        if (Device.IsIPhone)
            return OutputAssembly;
        if (Device.IsAndroid)
            return ServerExtensions.ExtractAndroidAssemblies(OutputAssembly);

        throw new NotSupportedException();
    }
    public BaseLaunchAgent GetLauchAgent() {
        if (ProfilerMode.EqualsInsensitive("trace"))
            return new TraceLaunchAgent(this);
        if (ProfilerMode.EqualsInsensitive("gcdump"))
            return new GCDumpLaunchAgent(this);
        if (!SkipDebug)
            return new DebugLaunchAgent(this);

        return new NoDebugLaunchAgent(this);
    }

    private static DebuggerSessionOptions GetDebuggerSessionOptions(JToken debuggerJsonToken) {
        var debuggerOptions = ServerExtensions.DefaultDebuggerOptions;
        var options = debuggerJsonToken.ToObject(DebuggerOptionsContext.Default.DebugOptions);
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
        debuggerOptions.EvaluationOptions.IntegerDisplayFormat = DebugOptions.GetIntegerDisplayFormat(options.IntegerDisplayFormat);
        debuggerOptions.ProjectAssembliesOnly = options.ProjectAssembliesOnly;
        debuggerOptions.StepOverPropertiesAndOperators = options.StepOverPropertiesAndOperators;
        debuggerOptions.SearchMicrosoftSymbolServer = options.SearchMicrosoftSymbolServer;
        debuggerOptions.SearchNuGetSymbolServer = options.SearchNuGetSymbolServer;
        debuggerOptions.SourceCodeMappings = options.SourceCodeMappings;
        debuggerOptions.AutomaticSourceLinkDownload = options.AutomaticSourceLinkDownload;
        debuggerOptions.SymbolSearchPaths = options.SymbolSearchPaths;

        return debuggerOptions;
    }
}
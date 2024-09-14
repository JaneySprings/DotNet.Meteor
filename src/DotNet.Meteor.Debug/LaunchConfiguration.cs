using DotNet.Meteor.Common;
using DotNet.Meteor.Debug.Extensions;
using Newtonsoft.Json.Linq;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug;

public class LaunchConfiguration {
    public Project Project { get; init; }
    public DeviceData Device { get; init; }
    public string Configuration { get; init; }

    public bool UninstallApp { get; init; }
    public bool SkipDebug { get; init; }
    public int DebugPort { get; init; }
    public int ReloadHostPort { get; init; }
    public int ProfilerPort { get; init; }
    public string? ProfilerMode { get; init; }
    public string ProgramPath { get; init; }
    public DebuggerSessionOptions DebuggerSessionOptions { get; init; }

    public LaunchConfiguration(Dictionary<string, JToken> configurationProperties) {
        Project = configurationProperties["project"].ToClass<Project>()!;
        Device = configurationProperties["device"].ToClass<DeviceData>()!;
        Configuration = configurationProperties["configuration"].ToClass<string>()!;
        
        UninstallApp = configurationProperties.TryGetValue("uninstallApp").ToValue<bool>();
        SkipDebug = configurationProperties.TryGetValue("skipDebug").ToValue<bool>();
        DebugPort = configurationProperties.TryGetValue("debuggingPort").ToValue<int>();
        ReloadHostPort = configurationProperties.TryGetValue("reloadHost").ToValue<int>();
        ProfilerPort = configurationProperties.TryGetValue("profilerPort").ToValue<int>();
        ProfilerMode = configurationProperties.TryGetValue("profilerMode")?.ToClass<string>();
        DebuggerSessionOptions = configurationProperties.TryGetValue("debuggerOptions")?.ToClass<DebuggerSessionOptions>() 
            ?? ServerExtensions.DefaultDebuggerOptions;

        DebugPort = DebugPort == 0 ? ServerExtensions.FindFreePort() : DebugPort;
        ReloadHostPort = ReloadHostPort == 0 ? ServerExtensions.FindFreePort() : ReloadHostPort;
        ProfilerPort = ProfilerPort == 0 ? ServerExtensions.FindFreePort() : ProfilerPort;

        ProgramPath = configurationProperties.TryGetValue("program").ToClass<string>() ?? string.Empty;
        if (!File.Exists(ProgramPath) && !Directory.Exists(ProgramPath))
            throw ServerExtensions.GetProtocolException($"Incorrect path to program: '{ProgramPath}'");
    }

    public string GetApplicationName() {
        if (!Device.IsAndroid)
            return Path.GetFileNameWithoutExtension(ProgramPath);

        var assemblyName = Path.GetFileNameWithoutExtension(ProgramPath);
        return assemblyName.Replace("-Signed", "");
    }
    public string GetApplicationAssembliesDirectory() {
        if (Device.IsMacCatalyst)
            return Path.Combine(ProgramPath, "Contents", "MonoBundle");
        if (Device.IsIPhone)
            return ProgramPath;
        if (Device.IsAndroid)
            return ServerExtensions.ExtractAndroidAssemblies(ProgramPath);

        throw new NotSupportedException();
    }
    public BaseLaunchAgent GetLaunchAgent() {
        if (ProfilerMode.EqualsInsensitive("trace"))
            return new TraceLaunchAgent(this);
        if (ProfilerMode.EqualsInsensitive("gcdump"))
            return new GCDumpLaunchAgent(this);
        if (!SkipDebug)
            return new DebugLaunchAgent(this);

        return new NoDebugLaunchAgent(this);
    }
}
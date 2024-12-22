using DotNet.Meteor.Common;
using DotNet.Meteor.Profiler.Extensions;
using Newtonsoft.Json.Linq;

namespace DotNet.Meteor.Profiler;

public class LaunchConfiguration {
    public Project Project { get; init; }
    public DeviceData Device { get; init; }
    public string ProgramPath { get; init; }
    public bool UninstallApp { get; init; }
    public int ProfilerPort { get; init; }

    private ProfilerMode Profiler { get; init; }

    public LaunchConfiguration(Dictionary<string, JToken> configurationProperties) {
        Project = configurationProperties["project"].ToClass<Project>()!;
        Device = configurationProperties["device"].ToClass<DeviceData>()!;
        
        UninstallApp = configurationProperties.TryGetValue("uninstallApp").ToValue<bool>();
        ProfilerPort = configurationProperties.TryGetValue("profilerPort").ToValue<int>();
        Profiler = configurationProperties.TryGetValue("profilerMode").ToValue<ProfilerMode>();
       
        ProfilerPort = ProfilerPort == 0 ? RuntimeSystem.GetFreePort() : ProfilerPort;

        ProgramPath = Project.GetRelativePath(configurationProperties.TryGetValue("program").ToClass<string>());
        if (!File.Exists(ProgramPath) && !Directory.Exists(ProgramPath))
            ProgramPath = FindProgramPath(ProgramPath); // Last chance to get program path
    }

    public string GetApplicationName() {
        if (!Device.IsAndroid)
            return Path.GetFileNameWithoutExtension(ProgramPath);

        var assemblyName = Path.GetFileNameWithoutExtension(ProgramPath);
        return assemblyName.Replace("-Signed", "");
    }
    public BaseLaunchAgent GetLaunchAgent() {
        if (Profiler == ProfilerMode.Trace)
            return new TraceLaunchAgent(this);
        if (Profiler == ProfilerMode.GCDump)
            return new GCDumpLaunchAgent(this);
        
        //return new UniversalLaunchAgent(this);
        return new TraceLaunchAgent(this);
    }

    private string FindProgramPath(string programPath) {
        if (string.IsNullOrEmpty(programPath))
            throw ServerExtensions.GetProtocolException("Program path is null or empty");

        var programDirectory = Path.GetDirectoryName(programPath)!;
        if (!Directory.Exists(programDirectory))
            throw ServerExtensions.GetProtocolException($"Incorrect path to program: '{ProgramPath}'");

        if (Device.IsAndroid) {
            var apkPaths = Directory.GetFiles(programDirectory, "*-Signed.apk");
            if (apkPaths.Length == 1)
                return apkPaths[0];
        }
        if (Device.IsMacCatalyst || Device.IsIPhone) {
            var appExtension = RuntimeSystem.IsMacOS ? ".app" : ".ipa";
            var appPaths = Directory.GetDirectories(programDirectory, $"*{appExtension}");
            if (appPaths.Length == 1)
                return appPaths[0];
        }

        throw ServerExtensions.GetProtocolException($"Incorrect path to program: '{ProgramPath}'");
    }

    private enum ProfilerMode { None, Trace, GCDump }
}
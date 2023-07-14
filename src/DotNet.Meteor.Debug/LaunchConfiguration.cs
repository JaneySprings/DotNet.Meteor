using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNet.Meteor.Shared;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Newtonsoft.Json.Linq;

namespace DotNet.Meteor.Debug;

public class LaunchConfiguration {
    public string Framework { get; private set; }
    public string OutputAssembly { get; private set; }
    public DeviceData Device { get; }
    public Project Project { get; }
    public string Target { get; }
    public bool UninstallApp { get; }
    public int ReloadHostPort { get; }
    public int DebugPort { get; set; }

    public bool IsDebug => Target.Equals("debug", StringComparison.OrdinalIgnoreCase);

    public LaunchConfiguration(Dictionary<string, JToken> configurationProperties) {
        Project = configurationProperties["selected_project"].ToObject<Project>();
        Device = configurationProperties["selected_device"].ToObject<DeviceData>();
        Target = configurationProperties["selected_target"].ToObject<string>();
        ReloadHostPort = configurationProperties["reload_host"].ToObject<int>();
        UninstallApp = configurationProperties["uninstall_app"].ToObject<bool>();
        DebugPort = configurationProperties["debugging_port"].ToObject<int>();
        
        Framework = Project.Frameworks.First(it => it.ContainsInsensitive(Device.Platform));
        OutputAssembly = Project.FindOutputApplication(Target, Framework, Device, message => {
            throw new ProtocolException($"Failed to load launch configuration. {message}");
        });
    }

    public string GetApplicationId() {
        // if (Device.IsIPhone || Device.IsMacCatalyst) {
        //     var workingDirectory = Path.GetDirectoryName(Project.Path);
        //     var files = Directory.GetFiles(workingDirectory, "Info.plist", SearchOption.AllDirectories)
        //         .Where(it => !it.Contains(Path.GetFileName(OutputAssembly)));

        //     if (!files.Any())
        //         return null;

        //     var plist = new PropertyExtractor(files.First());
        //     return plist.Extract("CFBundleIdentifier") ?? Project.EvaluateProperty("ApplicationId");
        // }

        if (!Device.IsAndroid)
            return null;

        var assemblyName = Path.GetFileNameWithoutExtension(OutputAssembly);
        return assemblyName.Replace("-Signed", "");
    }
}
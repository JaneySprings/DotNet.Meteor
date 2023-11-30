using System;
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
    public DebuggerSessionOptions DebuggerSessionOptions { get; }
    public DeviceData Device { get; }
    public Project Project { get; }

    public string OutputAssembly { get; }
    public string Framework { get; }
    public string Target { get; }
    public bool UninstallApp { get; }

    public int ReloadHostPort { get; }
    public int DebugPort { get; set; }

    public bool IsDebug => Target.Equals("debug", StringComparison.OrdinalIgnoreCase);


    public LaunchConfiguration(Dictionary<string, JToken> configurationProperties) {
        Project = configurationProperties["selected_project"].ToObject<Project>(TrimmableContext.Default.Project);
        Device = configurationProperties["selected_device"].ToObject<DeviceData>(TrimmableContext.Default.DeviceData);
        Target = configurationProperties["selected_target"].ToObject<string>(TrimmableContext.Default.String);
        ReloadHostPort = configurationProperties["reload_host"].ToObject<int>(TrimmableContext.Default.Int32);
        UninstallApp = configurationProperties["uninstall_app"].ToObject<bool>(TrimmableContext.Default.Boolean);
        DebugPort = configurationProperties["debugging_port"].ToObject<int>(TrimmableContext.Default.Int32);
        
        DebuggerSessionOptions = GetDebuggerSessionOptions(configurationProperties["debugger_options"]);
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
        debuggerOptions.EvaluationOptions.ChunkRawStrings = options.ChunkRawStrings;
        debuggerOptions.EvaluationOptions.IntegerDisplayFormat = DebuggerOptions.GetIntegerDisplayFormat(options.IntegerDisplayFormat);

        return debuggerOptions;
    }
}
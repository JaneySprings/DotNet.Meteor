using System;
using System.IO;
using System.Diagnostics;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Debug.Sdk;

public static class IDeviceTool {
    public static void Installer(string serial, string bundlePath, IProcessLogger logger = null) {
        var tool = new FileInfo(Path.Combine(AppleSdk.IDeviceLocation(), "ideviceinstaller.exe"));
        var result = new ProcessRunner(tool, new ProcessArgumentBuilder()
            .Append("--udid").Append(serial)
            .Append("--install").AppendQuoted(bundlePath)
            .Append("--notify-wait"), logger)
            .WaitForExit();

        if (!result.Success)
            throw new Exception(string.Join(Environment.NewLine, result.StandardError));
    }

    public static Process Debug(string serial, string bundleId, int port, IProcessLogger logger = null) {
        var tool = new FileInfo(Path.Combine(AppleSdk.IDeviceLocation(), "idevicedebug.exe"));
        return new ProcessRunner(tool, new ProcessArgumentBuilder()
            .Append("run").Append(bundleId)
            .Append("--udid").Append(serial)
            .Append("--env").Append($"__XAMARIN_DEBUG_PORT__={port}")
            .Append("--debug"), logger)
            .Start();
    }
}
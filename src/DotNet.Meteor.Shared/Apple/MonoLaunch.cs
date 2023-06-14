using System;
using System.IO;
using System.Diagnostics;
using DotNet.Meteor.Processes;

namespace DotNet.Meteor.Apple {
    public static class MonoLaunch {
        public static Process TcpTunnel(string serial, int port, IProcessLogger logger = null) {
            FileInfo tool = PathUtils.MLaunchTool();
            return new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append($"--tcp-tunnel={port}:{port}")
                .Append($"--devname={serial}"), logger)
                .Start();
        }

        public static void InstallDev(string serial, string bundlePath, IProcessLogger logger = null) {
            var tool = PathUtils.MLaunchTool();
            new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append( "--installdev").AppendQuoted(bundlePath)
                .Append($"--devname={serial}"), logger)
                .WaitForExit();
        }

        public static void LaunchDev(string serial, string bundlePath, IProcessLogger logger = null) {
            var tool = PathUtils.MLaunchTool();
            var arguments = new ProcessArgumentBuilder()
                .Append( "--launchdev").AppendQuoted(bundlePath)
                .Append($"--devname={serial}");
            var result = new ProcessRunner(tool, arguments, logger).WaitForExit();

            if (!result.Success)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));
        }

        public static Process LaunchSim(string serial, string bundlePath, IProcessLogger logger = null) {
            var tool = PathUtils.MLaunchTool();
            var arguments = new ProcessArgumentBuilder()
                .Append( "--launchsim").AppendQuoted(bundlePath)
                .Append($"--device=:v2:udid={serial}");
            return new ProcessRunner(tool, arguments, logger).Start();
        }

        public static Process DebugDev(string serial, string bundlePath, int port, IProcessLogger logger = null) {
            var tool = PathUtils.MLaunchTool();
            var arguments = new ProcessArgumentBuilder()
                .Append( "--launchdev").AppendQuoted(bundlePath)
                .Append($"--devname={serial}")
                .Append( "--argument=-monodevelop-port")
                .Append($"--argument={port}")
                .Append($"--setenv=__XAMARIN_DEBUG_PORT__={port}")
                .Append( "--wait-for-exit");
            return new ProcessRunner(tool, arguments, logger).Start();
        }

        public static Process DebugSim(string serial, string bundlePath, int port, IProcessLogger logger = null) {
            var tool = PathUtils.MLaunchTool();
            var arguments = new ProcessArgumentBuilder()
                .Append( "--launchsim").AppendQuoted(bundlePath)
                .Append( "--argument=-monodevelop-port")
                .Append($"--argument={port}")
                .Append($"--setenv=__XAMARIN_DEBUG_PORT__={port}")
                .Append($"--device=:v2:udid={serial}");
            return new ProcessRunner(tool, arguments, logger).Start();
        }
    }
}
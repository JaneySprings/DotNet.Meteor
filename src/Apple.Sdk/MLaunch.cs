using System.IO;
using System.Diagnostics;
using DotNet.Mobile.Shared;

namespace Apple.Sdk {
    public static class MLaunch {
        public static void InstallOnDevice(string bundlePath, string serial, IProcessLogger logger = null) {
            FileInfo tool = PathUtils.MLaunchTool();
            new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append("--installdev").AppendQuoted(bundlePath)
                .Append("--devname", serial), logger)
                .WaitForExit();
        }

        public static Process LaunchOnDevice(string bundlePath, string serial, int port, IProcessLogger logger = null) {
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

        public static Process LaunchOnSimulator(string bundlePath, string serial, int port, IProcessLogger logger = null) {
            var tool = PathUtils.MLaunchTool();
            var arguments = new ProcessArgumentBuilder()
                .Append( "--launchsim").AppendQuoted(bundlePath)
                .Append( "--argument=-monodevelop-port")
                .Append($"--argument={port}")
                .Append($"--setenv=__XAMARIN_DEBUG_PORT__={port}")
                .Append($"--device=:v2:udid={serial}");
            return new ProcessRunner(tool, arguments, logger).Start();
        }

        public static Process TcpTunnel(string serial, int port) {
            FileInfo tool = PathUtils.MLaunchTool();
            return new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append($"--tcp-tunnel={port}:{port}")
                .Append($"--devname={serial}"))
                .Start();
        }
    }
}
using System.IO;
using System.Diagnostics;
using DotNet.Mobile.Shared;

namespace Apple.Sdk {
    public static class MLaunch {
        public static void InstallOnDevice(string bundlePath, DeviceData device) {
            FileInfo tool = PathUtils.MLaunchTool();
            new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append("--installdev", $"\"{bundlePath}\"")
                .Append("--devname", device.Serial))
                .WaitForExit();
        }

        public static Process TcpTunnel(DeviceData device, int port) {
            FileInfo tool = PathUtils.MLaunchTool();
            return new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append($"--tcp-tunnel={port}:{port}")
                .Append($"--devname={device.Serial}"))
                .Start();
        }

        public static Process LaunchAppForDebug(string bundlePath, DeviceData device, int port, IProcessLogger logger = null) {
            FileInfo tool = PathUtils.MLaunchTool();
            ProcessRunner process;

            if (device.IsEmulator) {
                process = new ProcessRunner(tool, new ProcessArgumentBuilder()
                    .Append( "--launchsim", $"\"{bundlePath}\"")
                    .Append( "--argument=-monodevelop-port")
                    .Append($"--argument={port}")
                    .Append($"--setenv=__XAMARIN_DEBUG_PORT__={port}")
                    .Append($"--device=:v2:udid={device.Serial}"),
                    logger
                );
            } else {
                InstallOnDevice(bundlePath, device);
                process = new ProcessRunner(tool, new ProcessArgumentBuilder()
                    .Append( "--launchdev", $"\"{bundlePath}\"")
                    .Append( "--devname", device.Serial)
                    .Append( "--argument=-monodevelop-port")
                    .Append($"--argument={port}")
                    .Append($"--setenv=__XAMARIN_DEBUG_PORT__={port}")
                    .Append( "--wait-for-exit"),
                    logger
                );
            }
            return process.Start();
        }
    }
}
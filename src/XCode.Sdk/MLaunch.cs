using System.IO;
using DotNet.Mobile.Shared;

namespace XCode.Sdk {
    public static class MLaunch {
        public static void InstallOnDevice(string bundlePath, DeviceData device) {
            FileInfo tool = PathUtils.MLaunchTool();
            ProcessRunner.Execute(tool, new ProcessArgumentBuilder()
                .Append("--installdev", $"\"{bundlePath}\"")
                .Append("--devname", device.Serial)
            );
        }

        public static void TcpTunnel(DeviceData device, int port) {
            FileInfo tool = PathUtils.MLaunchTool();
            var proccess = new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append($"--tcp-tunnel={port}:{port}")
                .Append($"--devname={device.Serial}")
            );
            proccess.Run();
        }

        public static void LaunchAppForDebug(string bundlePath, DeviceData device, int port) {
            FileInfo tool = PathUtils.MLaunchTool();
            ProcessRunner process;

            if (device.IsEmulator) {
                process = new ProcessRunner(tool, new ProcessArgumentBuilder()
                    .Append( "--launchsim", $"\"{bundlePath}\"")
                    .Append( "--argument=-monodevelop-port")
                    .Append($"--argument={port}")
                    .Append($"--setenv=__XAMARIN_DEBUG_PORT__={port}")
                    .Append($"--device=:v2:udid={device.Serial}"), redirectStandardInput: true
                );
            } else {
                TcpTunnel(device, port);
                InstallOnDevice(bundlePath, device);
                process = new ProcessRunner(tool, new ProcessArgumentBuilder()
                    .Append( "--launchdev", $"\"{bundlePath}\"")
                    .Append( "--devname", device.Serial)
                    .Append( "--argument=-monodevelop-port")
                    .Append($"--argument={port}")
                    .Append($"--setenv=__XAMARIN_DEBUG_PORT__={port}")
                    .Append( "--wait-for-exit"), redirectStandardInput: true
                );
            }
            process.Run();
        }
    }
}
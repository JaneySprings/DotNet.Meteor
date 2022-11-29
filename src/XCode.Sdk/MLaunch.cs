using System.IO;
using System.Threading;
using DotNet.Mobile.Shared;

namespace XCode.Sdk {
    public static class MLaunch {
        public static void InstallOnDevice(string bundlePath, DeviceData device) {
            FileInfo tool = PathUtils.MLaunchTool();
            ProcessRunner.Run(tool, new ProcessArgumentBuilder()
                .Append($"--installdev", $"\"{bundlePath}\"")
                .Append($"--devname={device.Serial}")
            );
        }

        public static void TcpTunnel(DeviceData device, int port) {
            FileInfo tool = PathUtils.MLaunchTool();
            var proccess = new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append($"--tcp-tunnel={port}:{port}")
                .Append($"--devname={device.Serial}")
            );
            proccess.WaitForExitAsync();
        }

        public static void LaunchAppForDebug(string bundlePath, DeviceData device, int port) {
            FileInfo tool = PathUtils.MLaunchTool();

            if (!device.IsEmulator) {
                TcpTunnel(device, port);
                InstallOnDevice(bundlePath, device);
            }

            var platform = device.IsEmulator ? "sim" : "dev";
            var process = new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append($"--launch{platform}", $"\"{bundlePath}\"")
                .Append($"--argument=-monodevelop-port --argument={port} --setenv=__XAMARIN_DEBUG_PORT__={port}")
                .Append($"--device=:v2:udid={device.Serial}"), CancellationToken.None, redirectStandardInput: true
            );
            process.WaitForExitAsync();
        }
    }
}
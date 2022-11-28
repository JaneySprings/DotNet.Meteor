using System;
using System.IO;
using System.Threading;
using DotNet.Mobile.Shared;

namespace XCode.Sdk {
    public static class MLaunch {
        public static FileInfo ToolLocation() {
            string dotnetPath = Path.Combine("usr", "local", "share", "dotnet");
            string sdkPath = Path.Combine(dotnetPath, "packs", "Microsoft.iOS.Sdk");
            FileInfo newestTool = null;

            foreach (string directory in Directory.GetDirectories(sdkPath)) {
                string mlaunchPath = Path.Combine(directory, "tools", "bin", "mlaunch");

                if (File.Exists(mlaunchPath)) {
                    var tool = new FileInfo(mlaunchPath);

                    if (newestTool == null || tool.CreationTime > newestTool.CreationTime)
                        newestTool = tool;
                }
            }

            if (newestTool == null || !newestTool.Exists)
                throw new Exception("Could not find mlaunch tool");

            return newestTool;
        }

        public static void InstallOnDevice(string bundlePath, DeviceData device) {
            FileInfo tool = MLaunch.ToolLocation();
            ProcessRunner.Run(tool, new ProcessArgumentBuilder()
                .Append($"--installdev", $"\"{bundlePath}\"")
                .Append($"--devname={device.Serial}")
            );
        }

        public static void TcpTunnel(DeviceData device, int port) {
            FileInfo tool = MLaunch.ToolLocation();
            var proccess = new ProcessRunner(tool, new ProcessArgumentBuilder()
                .Append($"--tcp-tunnel={port}:{port}")
                .Append($"--devname={device.Serial}")
            );
            proccess.WaitForExitAsync();
        }

        public static void LaunchAppForDebug(string bundlePath, DeviceData device, int port) {
            FileInfo tool = MLaunch.ToolLocation();

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
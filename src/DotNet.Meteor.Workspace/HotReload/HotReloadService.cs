using System.Diagnostics;
using DotNet.Debugging.Common.Interop;
using DotNet.Debugging.Common.Logging;
using DotNet.Meteor.HotReload.Providers;

namespace DotNet.Meteor.Workspace.HotReload;

public static class HotReloadService {
    public static async Task ConfigureAndRun(int hostPid, int port, string? mode = "universal") {
        IConnectionProvider? provider = null;
        if (mode == "universal")
            provider = new UniversalConnectionProvider(port);
        // Not used ?
        if (mode == "android")
            provider = new AndroidConnectionProvider(port, mode);
        if (mode == "ios")
            provider = new AppleConnectionProvider(port, mode);

        ArgumentNullException.ThrowIfNull(provider, "Invalid mode");
        ObserveClientProcess(hostPid);
        await Run(provider!);
    }


    private static async Task Run(IConnectionProvider provider) {
        var hotReloadClient = new HotReloadClient(provider, new ProcessLogger());
        // TODO: We don't know android serial if device is not booted
        // if (!await hotReloadClient.PrepareTransportAsync()) {
        //     logger.Error("Failed to prepare transport");
        //     return;
        // }
        while (true) {
            var filePath = Console.ReadLine();
            if (string.IsNullOrEmpty(filePath)) {
                CurrentSessionLogger.Debug("Received empty file path, exiting");
                break;
            }
            await hotReloadClient.SendNotificationAsync(filePath);
        }
    }
    private static void ObserveClientProcess(int pid) {
        var ideProcess = Process.GetProcessById(pid);
        ideProcess.EnableRaisingEvents = true;
        ideProcess.Exited += (_, _) => {
            CurrentSessionLogger.Debug($"Shutting down server because client process has exited");
            Environment.Exit(0);
        };
        CurrentSessionLogger.Debug($"Server is observing client process {ideProcess.ProcessName} (PID: {pid})");
    }

    private sealed class ProcessLogger : IProcessLogger {
        public void OnErrorDataReceived(string stderr) {
            CurrentSessionLogger.Error(stderr);
        }
        public void OnOutputDataReceived(string stdout) {
            CurrentSessionLogger.Debug(stdout);
        }
    }
}

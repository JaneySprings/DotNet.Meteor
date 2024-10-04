using DotNet.Meteor.Common;
using DotNet.Meteor.Common.Processes;
using DotNet.Meteor.HotReload.Providers;
using NLog;
using System.Diagnostics;
using System.Reflection;

namespace DotNet.Meteor.HotReload;

public class Program {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private static async Task Main(string[] args) {
        if (args.Length == 0) {
            Help(args);
            return;
        }

        LogConfig.InitializeLog();
        var hostPid = int.Parse(args[0]);
        var port = int.Parse(args[1]);
        var mode = args[2];
    
        IConnectionProvider? provider = null;
        if (mode == "universal") 
            provider = new UniversalConnectionProvider(port);
        if (mode == "android") 
            provider = new AndroidConnectionProvider(port, args[2]);
        if (mode == "ios")
            provider = new AppleConnectionProvider(port, args[2]);

        ArgumentNullException.ThrowIfNull(provider, "Invalid mode");
        ObserveClientProcess(hostPid);
        await Run(provider!);
    }

    public static void Help(string[] args) {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var name = Assembly.GetExecutingAssembly().GetName().Name;
        Console.WriteLine($"{name} version {version?.Major}.{version?.Minor}.{version?.Build} for Visual Studio Code");
        Console.WriteLine("Copyright (C) Nikita Romanov. All rights reserved.");
    }
    private static async Task Run(IConnectionProvider provider) {
        var hotReloadClient = new HotReloadClient(provider, new ProcessLogger(logger));
        // TODO: We don't know android serial if device is not booted
        // if (!await hotReloadClient.PrepareTransportAsync()) {
        //     logger.Error("Failed to prepare transport");
        //     return;
        // }
        while (true) {
            var filePath = Console.ReadLine();
            if (string.IsNullOrEmpty(filePath)) {
                logger.Debug("Received empty file path, exiting");
                break;
            }
            await hotReloadClient.SendNotificationAsync(filePath);
        }
    }
    private static void ObserveClientProcess(int pid) {
        var ideProcess = Process.GetProcessById(pid);
        ideProcess.EnableRaisingEvents = true;
        ideProcess.Exited += (_, _) => {
            logger.Debug($"Shutting down server because client process has exited");
            Environment.Exit(0);
        };
        logger.Debug($"Server is observing client process {ideProcess.ProcessName} (PID: {pid})");
    }

    private class ProcessLogger : IProcessLogger {
        private readonly Logger logger;
        public ProcessLogger(Logger logger) {
            this.logger = logger;
        }

        public void OnErrorDataReceived(string stderr) {
            logger.Error(stderr);
        }
        public void OnOutputDataReceived(string stdout) {
            logger.Info(stdout);
        }
    }
}

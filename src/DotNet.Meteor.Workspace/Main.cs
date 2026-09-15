using System.CommandLine;
using System.Text.Json;
using DotNet.Debugging.Common.Android;
using DotNet.Debugging.Common.Logging;
using DotNet.Meteor.Workspace.Devices;
using DotNet.Meteor.Workspace.HotReload;

namespace DotNet.Meteor.Workspace;

public class Program {
    public static int Main(string[] args) {
        var initializeOption = new Option<bool>("--initialize", "-init");
        var allDevicesOption = new Option<bool>("--all-devices", "-dev");
        var androidSdkOption = new Option<bool>("--android-sdk-path", "-ahome");
        var rootCommand = new RootCommand(".NET Meteor Workspace") {
            Options = {
                initializeOption,
                allDevicesOption,
                androidSdkOption,
            },
            Subcommands = {
                CreateHotReloadCommand(),
            }
        };
        rootCommand.SetAction(result => {
            if (result.GetValue(allDevicesOption)) {
                var devices = DeviceProvider.GetDevices(CurrentSessionLogger.Error, CurrentSessionLogger.Debug);
                Console.WriteLine(JsonSerializer.Serialize(devices));
                return;
            }
            if (result.GetValue(androidSdkOption)) {
                string path = AndroidSdkLocator.GetSdkDirecotry();
                Console.WriteLine(path);
                return;
            }
            if (result.GetValue(initializeOption)) {
                // start android daemon (workaround for nodejs child_process hanging issue)
                try { AndroidDebugBridge.StartServer(); } catch { }
                // TODO: usbmuxd
                // run usbmuxd manually
                Console.WriteLine(JsonSerializer.Serialize(true));
                return;
            }
        });

        return rootCommand.Parse(args).Invoke();
    }

    private static Command CreateHotReloadCommand() {
        var hostPidOption = new Option<int>("--host-pid", "-host");
        var portOption = new Option<int>("--port", "-p");
        var modeOption = new Option<string>("--mode", "-m");
        var hotReloadCommand = new Command("hotreload") {
            Options = {
                hostPidOption,
                portOption,
                modeOption,
            }
        };
        hotReloadCommand.SetAction(result => {
            var hostPid = result.GetValue(hostPidOption);
            var port = result.GetValue(portOption);
            var mode = result.GetValue(modeOption);
            HotReloadService.ConfigureAndRun(hostPid, port, mode).Wait();
        });
        return hotReloadCommand;
    }
}

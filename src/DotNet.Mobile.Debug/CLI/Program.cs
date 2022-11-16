using System;
using System.Reflection;

namespace DotNet.Mobile.Debug.CLI;

public class Program {
    public static string Version {
        get {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version?.Major}.{version?.Minor}.{version?.Build}";
        }
    }

    private static void Main(string[] args) {
        if (args.Length == 0) {
            Command.Help();
            return;
        }

        switch (args[0].ToLower()) {
            case "--help":              Command.Help(); break;
            case "--version":           Command.Version(); break;
            case "--android-devices":   Command.AndroidDevices(); break;
            case "--apple-devices":     Command.AppleDevices(); break;
            case "--run-emulator":      Command.RunEmulator(args); break;
            case "--devices":           Command.AllDevices(); break;
            case "--free-port":         Command.FreePort(); break;
            case "--start-session":     Command.StartSession(); break;
            default: Command.Error(args[0]); break;
        }
    }
}

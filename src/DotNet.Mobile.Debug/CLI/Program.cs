using System;
using System.Reflection;

namespace DotNet.Mobile.Debug.CLI {
    public class Program {
        public static string Version {
            get {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
        }

        private static void Main(string[] args) {
            if (args.Length == 0) {
                Utils.CommandHelp();
                return;
            }

            switch (args[0].ToLower()) {
                case "--help":              Utils.CommandHelp(); break;
                case "--version":           Utils.CommandVersion(); break;
                case "--android-devices":   Utils.CommandAndroidDevices(); break;
                case "--apple-devices":     Utils.CommandAppleDevices(); break;
                case "--devices":           Utils.CommandAllDevices(); break;
                case "--start-session":     Utils.CommandStartSession(); break;
                default: Utils.CommandError(args[0]); break;
            }
        }
    }
}
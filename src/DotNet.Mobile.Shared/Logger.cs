using System;
using System.IO;

namespace DotNet.Mobile.Shared {
    public static class Logger {
        private const string Path = "/Users/nromanov/Work/vscode-meteor/extension/log.txt";

        static Logger() {
            if (File.Exists(Path))
                File.Delete(Path);
            using StreamWriter sw = File.CreateText(Path);
            sw.WriteLine("|" + DateTime.Now.ToString() + "| Start logging");
        }

        public static void Log(string message) {
            using StreamWriter sw = File.AppendText(Path);
            sw.WriteLine("|" + DateTime.Now.ToString() + "| " + message);
        }
    }
}
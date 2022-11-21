using System;
using System.IO;
using System.Reflection;

namespace DotNet.Mobile.Shared {
    public static class Logger {
        public static string LogStagingDirectory => "/Users/nromanov/Work/vscode-meteor/extension";
        private static readonly string LogFile = Path.Combine(LogStagingDirectory, "session.log");

        static Logger() {
            foreach (var log in Directory.GetFiles($"{LogStagingDirectory}/", "*.log")) {
                File.Delete(log);
            }
            using StreamWriter sw = File.CreateText(LogFile);
            sw.WriteLine("|" + DateTime.UtcNow + "| Start logging");
        }

        public static void Log(string format, params object[] args) {
            WriteInFile(string.Format(format, args));
        }
        public static void Log(Exception e) {
            WriteInFile($"{e.Message}\n{e.StackTrace}");
        }
        public static void Log(string message) {
            WriteInFile(message);
        }

        private static void WriteInFile(string message) {
            using StreamWriter sw = File.AppendText(LogFile);
            sw.WriteLine("|" + DateTime.Now.ToString() + "| " + message);
        }
    }
}
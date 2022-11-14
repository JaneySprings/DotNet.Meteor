using System;
using System.IO;
using System.Reflection;

namespace DotNet.Mobile.Shared {
    public static class Logger {
        private static readonly string LogFile = Path.Combine(
            "/Users/nromanov/Work/vscode-meteor/extension", "debug_session.log"
        );

        static Logger() {
            if (File.Exists(LogFile))
                File.Delete(LogFile);
            using StreamWriter sw = File.CreateText(LogFile);
            sw.WriteLine("|" + DateTime.Now.ToString() + "| Start logging");
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
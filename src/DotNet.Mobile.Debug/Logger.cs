using System;
using System.IO;

namespace DotNet.Mobile.Debug {
    public static class Logger {
        public static string LogStagingDirectory => AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string LogFile = Path.Combine(LogStagingDirectory, "session.log");

        static Logger() {
            if (File.Exists(LogFile))
                File.Delete(LogFile);

            WriteInFile("Session started");
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
using System;
using System.IO;
using DotNet.Meteor.Processes;

namespace DotNet.Meteor.Shared {
    public class Logger: IProcessLogger {
        private string LogStagingDirectory => AppDomain.CurrentDomain.BaseDirectory;
        public string LogFile => Path.Combine(LogStagingDirectory, $"{tag}.log");
        private readonly string tag;

        public Logger(string tag) {
            this.tag = tag;
            if (File.Exists(LogFile))
                File.Delete(LogFile);
        }

        public void OnOutputDataReceived(string stdout) {
            Log(stdout);
        }
        public void OnErrorDataReceived(string stderr) {
            Log(stderr);
        }

        public void Log(string format, params object[] args) {
            WriteInFile(string.Format(format, args));
        }
        public void Log(Exception e) {
            WriteInFile($"{e.Message}\n{e.StackTrace}");
        }
        public void Log(string message) {
            WriteInFile(message);
        }

        private void WriteInFile(string message) {
            using StreamWriter sw = File.AppendText(LogFile);
            sw.WriteLine("|" + DateTime.Now.ToString() + "| " + message);
        }

    }
}
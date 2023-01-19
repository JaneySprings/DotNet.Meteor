using System;
using System.IO;
using DotNet.Meteor.Processes;

namespace DotNet.Meteor.Android {
    internal class EmulatorLogger: IProcessLogger {
        private static string LogStagingDirectory => AppDomain.CurrentDomain.BaseDirectory;
        private readonly string LogStandardOutputFile = Path.Combine(LogStagingDirectory, "session_emu_stdout.log");
        private readonly string LogStandardErrorFile = Path.Combine(LogStagingDirectory, "session_emu_stderr.log");


        internal EmulatorLogger() {
            if (File.Exists(LogStandardOutputFile))
                File.Delete(LogStandardOutputFile);

            if (File.Exists(LogStandardErrorFile))
                File.Delete(LogStandardErrorFile);
        }

        public void OnOutputDataReceived(string stderr) {
            using StreamWriter sw = File.AppendText(LogStandardOutputFile);
            sw.WriteLine("|" + DateTime.Now.ToString() + "| " + stderr);
        }

        public void OnErrorDataReceived(string stderr) {
            using StreamWriter sw = File.AppendText(LogStandardErrorFile);
            sw.WriteLine("|" + DateTime.Now.ToString() + "| " + stderr);
        }
    }
}
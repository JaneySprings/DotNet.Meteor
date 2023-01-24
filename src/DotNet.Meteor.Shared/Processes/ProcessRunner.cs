using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DotNet.Meteor.Processes {
    public class ProcessRunner {
        private List<string> standardOutput;
        private List<string> standardError;
        private readonly Process process;

        public ProcessRunner(FileInfo executable, ProcessArgumentBuilder builder = null, IProcessLogger logger = null) {
            this.process = new Process();
            this.process.StartInfo.Arguments = builder?.ToString();
            this.process.StartInfo.FileName = executable.FullName;
            this.process.StartInfo.WorkingDirectory = executable.DirectoryName;

            SetupProcessLogging(logger);
        }
        public ProcessRunner(string command, ProcessArgumentBuilder builder = null) {
            this.process = new Process();
            this.process.StartInfo.Arguments = builder?.ToString();
            this.process.StartInfo.FileName = command;

            SetupProcessLogging(null);
        }

        private void SetupProcessLogging(IProcessLogger logger = null) {
            this.standardOutput = new List<string>();
            this.standardError = new List<string>();
            this.process.StartInfo.CreateNoWindow = true;
            this.process.StartInfo.UseShellExecute = false;
            this.process.StartInfo.RedirectStandardOutput = true;
            this.process.StartInfo.RedirectStandardError = true;
            this.process.StartInfo.RedirectStandardInput = true;
            this.process.OutputDataReceived += (s, e) => {
                if (e.Data != null) {
                    if (logger != null)
                        logger.OnOutputDataReceived(e.Data);
                    else this.standardOutput.Add(e.Data);
                }
            };
            this.process.ErrorDataReceived += (s, e) => {
                if (e.Data != null) {
                    if (logger != null)
                        logger.OnErrorDataReceived(e.Data);
                    else this.standardError.Add(e.Data);
                }
            };
        }

        public void SetEnvironmentVariable(string key, string value) {
            this.process.StartInfo.EnvironmentVariables[key] = value;
        }

        public void Kill() {
            this.process?.Kill();
        }
        public Process Start() {
            this.process.Start();
            this.process.BeginOutputReadLine();
            this.process.BeginErrorReadLine();
            return this.process;
        }

        public ProcessResult WaitForExit() {
            this.Start();
            this.process.WaitForExit();
            return new ProcessResult(this.standardOutput, this.standardError, this.process.ExitCode);
        }
    }
}
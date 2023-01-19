using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DotNet.Meteor.Processes {
    public class ProcessRunner {
        readonly List<string> standardOutput;
        readonly List<string> standardError;
        readonly Process process;

        public ProcessRunner(FileInfo executable, ProcessArgumentBuilder builder = null, IProcessLogger logger = null) {
            this.standardOutput = new List<string>();
            this.standardError = new List<string>();

            this.process = new Process();
            this.process.StartInfo.FileName = executable.FullName;
            this.process.StartInfo.Arguments = builder != null ? builder.ToString() : string.Empty;
            this.process.StartInfo.CreateNoWindow = true;
            this.process.StartInfo.UseShellExecute = false;
            this.process.StartInfo.RedirectStandardOutput = true;
            this.process.StartInfo.RedirectStandardError = true;
            this.process.StartInfo.RedirectStandardInput = true;

            this.process.OutputDataReceived += (s, e) => {
                if (e.Data != null) {
                    this.standardOutput.Add(e.Data);
                    logger?.OnOutputDataReceived(e.Data);
                }
            };
            this.process.ErrorDataReceived += (s, e) => {
                if (e.Data != null) {
                    this.standardError.Add(e.Data);
                    logger?.OnErrorDataReceived(e.Data);
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
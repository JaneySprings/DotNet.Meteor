using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotNet.Mobile.Shared {
    public class ProcessRunner {
        readonly List<string> standardOutput;
        readonly List<string> standardError;
        readonly Process process;

        public int ExitCode => this.process.HasExited ? this.process.ExitCode : -1;
        public bool HasExited => this.process?.HasExited ?? false;

        public static ProcessResult Run(FileInfo exe, ProcessArgumentBuilder builder) {
            var p = new ProcessRunner(exe, builder);
            return p.WaitForExit();
        }

        public ProcessRunner(FileInfo executable, ProcessArgumentBuilder builder)
            : this(executable, builder, System.Threading.CancellationToken.None) { }

        public ProcessRunner(FileInfo executable, ProcessArgumentBuilder builder, System.Threading.CancellationToken cancelToken, bool redirectStandardInput = false) {
            this.standardOutput = new List<string>();
            this.standardError = new List<string>();

            this.process = new Process();
            this.process.StartInfo.FileName = executable.FullName;
            this.process.StartInfo.Arguments = builder.ToString();
            this.process.StartInfo.CreateNoWindow = true;
            this.process.StartInfo.UseShellExecute = false;
            this.process.StartInfo.RedirectStandardOutput = true;
            this.process.StartInfo.RedirectStandardError = true;

            if (redirectStandardInput)
                this.process.StartInfo.RedirectStandardInput = true;

            this.process.OutputDataReceived += (s, e) => {
                if (e.Data != null)
                    this.standardOutput.Add(e.Data);
            };
            this.process.ErrorDataReceived += (s, e) => {
                if (e.Data != null)
                    this.standardError.Add(e.Data);
            };
            this.process.Start();
            this.process.BeginOutputReadLine();
            this.process.BeginErrorReadLine();

            if (cancelToken != System.Threading.CancellationToken.None) {
                cancelToken.Register(() => {
                    try { this.process.Kill(); } catch { }
                });
            }
        }

        public void Kill() {
            this.process?.Kill();
        }

        public ProcessResult WaitForExit() {
            this.process.WaitForExit();
            return new ProcessResult(this.standardOutput, this.standardError, this.process.ExitCode);
        }

        public Task<ProcessResult> WaitForExitAsync() {
            var tcs = new TaskCompletionSource<ProcessResult>();

            Task.Run(() => {
                ProcessResult r = WaitForExit();
                tcs.TrySetResult(r);
            });

            return tcs.Task;
        }
    }
}
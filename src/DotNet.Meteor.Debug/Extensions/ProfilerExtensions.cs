using System.CommandLine;
using System.CommandLine.IO;
using System.Text;
using DotNet.Meteor.Common.Processes;

namespace DotNet.Meteor.Debug.Extensions;

public class ProfilerTask {
    private readonly CancellationTokenSource cancellationTokenSource;
    private readonly Task profilingTask;

    public ProfilerTask(Task profilingTask, CancellationTokenSource cancellationTokenSource) {
        this.cancellationTokenSource = cancellationTokenSource;
        this.profilingTask = profilingTask;
    }

    public void Terminate() {
        cancellationTokenSource.Cancel();
        profilingTask.Wait();
    }
}

public class CatchStartLogger : IProcessLogger {
    private readonly IProcessLogger innerLogger;
    private readonly Action onCatchStart;

    private const string CatchTarget = "The runtime has been configured to pause";

    public CatchStartLogger(IProcessLogger innerLogger, Action onCatchStart) {
        this.innerLogger = innerLogger;
        this.onCatchStart = onCatchStart;
    }

    void IProcessLogger.OnErrorDataReceived(string stderr) {
        innerLogger.OnErrorDataReceived(stderr);
        if (stderr.Contains(CatchTarget, StringComparison.OrdinalIgnoreCase))
            onCatchStart();
    }

    void IProcessLogger.OnOutputDataReceived(string stdout) {
        innerLogger.OnOutputDataReceived(stdout);
        if (stdout.Contains(CatchTarget, StringComparison.OrdinalIgnoreCase))
            onCatchStart();
    }
}

public class ConsoleLogger : IConsole {
    private IStandardStreamWriter _out;
    public IStandardStreamWriter Out => _out;

    private IStandardStreamWriter _error;
    public IStandardStreamWriter Error => _error;

    public bool IsOutputRedirected => false;
    public bool IsErrorRedirected => false;
    public bool IsInputRedirected => false;

    public ConsoleLogger(IProcessLogger processLogger) {
        _out = StandardStreamWriter.Create(new StringWriter(processLogger.OnOutputDataReceived));
        _error = StandardStreamWriter.Create(new StringWriter(processLogger.OnErrorDataReceived));
    }

    private class StringWriter : TextWriter {
        private readonly Action<string> handler;

        public StringWriter(Action<string> handler) {
            this.handler = handler;
        }

        public override Encoding Encoding => Encoding.UTF8;
        public override void Write(string? value) {
            handler.Invoke(value ?? string.Empty);
        }
    }
}
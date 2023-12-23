using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Meteor.Processes;

namespace DotNet.Meteor.Debug.Extensions;

public class ProfilingTask {
    private readonly CancellationTokenSource cancellationTokenSource;
    private readonly Task profilingTask;

    public ProfilingTask(Task profilingTask, CancellationTokenSource cancellationTokenSource) {
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
        if (stderr.Contains(CatchTarget))
            onCatchStart();
    }

    void IProcessLogger.OnOutputDataReceived(string stdout) {
        innerLogger.OnOutputDataReceived(stdout);
        if (stdout.Contains(CatchTarget))
            onCatchStart();
    }
}
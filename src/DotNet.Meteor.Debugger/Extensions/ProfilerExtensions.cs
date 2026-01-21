using System.Diagnostics;
using DotNet.Meteor.Common.Processes;

namespace DotNet.Meteor.Debugger.Extensions;

public class ProfilerTask {
    private readonly Process profilerProcess;

    public ProfilerTask(Process profilerProcess) {
        this.profilerProcess = profilerProcess;
    }

    public void Terminate() {
        profilerProcess.StandardInput.WriteLine(Environment.NewLine);
        profilerProcess.WaitForExit();
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

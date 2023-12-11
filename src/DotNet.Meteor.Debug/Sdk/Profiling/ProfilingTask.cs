using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Meteor.Debug.Sdk.Profiling;

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
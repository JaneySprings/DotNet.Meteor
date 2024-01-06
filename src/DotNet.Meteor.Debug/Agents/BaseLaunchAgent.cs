using System;
using System.Collections.Generic;
using DotNet.Meteor.Processes;
using Mono.Debugging.Soft;

namespace DotNet.Meteor.Debug;

public abstract class BaseLaunchAgent {
    public List<Action> Disposables { get; init; }
    protected LaunchConfiguration Configuration { get; init; }

    protected BaseLaunchAgent(LaunchConfiguration configuration) {
        Disposables = new List<Action>();
        Configuration = configuration;
    }

    public abstract void Connect(SoftDebuggerSession session);
    public abstract void Launch(IProcessLogger logger);

    public virtual void Dispose() {
        foreach(var disposable in Disposables)
            disposable.Invoke();

        Disposables.Clear();
    }
}
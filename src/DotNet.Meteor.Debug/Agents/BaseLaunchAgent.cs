using System;
using System.Collections.Generic;
using DotNet.Meteor.Processes;
using Mono.Debugging.Soft;

namespace DotNet.Meteor.Debug;

public abstract class BaseLaunchAgent {
    public List<Action> Disposables { get; set; }

    protected BaseLaunchAgent() {
        Disposables = new List<Action>();
    }

    public abstract void Connect(SoftDebuggerSession session, LaunchConfiguration configuration);
    public abstract void Launch(LaunchConfiguration configuration, IProcessLogger logger);

    public virtual void Dispose() {
        foreach(var disposable in Disposables)
            disposable.Invoke();

        Disposables.Clear();
    }
}
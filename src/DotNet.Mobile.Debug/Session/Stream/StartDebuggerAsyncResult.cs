using System;
using System.IO;

namespace DotNet.Mobile.Debug.Session;

class StartDebuggerAsyncResult : AggregateAsyncResult {
    public StartDebuggerAsyncResult(AsyncCallback callback, object state)
        : base(callback, state) {
    }

    public Stream Transport, Output;
    public IAsyncResult RunningCommand;
    public bool Cancelled;
}
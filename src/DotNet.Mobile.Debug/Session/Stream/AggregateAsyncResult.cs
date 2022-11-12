using System;
using System.Threading;

namespace DotNet.Mobile.Debug.Session;

public class AggregateAsyncResult : IAsyncResult {
    AsyncCallback callback;
    object state;

    public AggregateAsyncResult(AsyncCallback callback, object state) {
        this.callback = callback;
        this.state = state;
    }

    public void Complete() {
        MarkCompleted();
        if (this.callback != null)
            this.callback(this);
    }

    public void CompleteWithError(Exception error) {
        Error = error;
        Complete();
    }

    public void CheckError(bool cancelled = false) {
        if (!IsCompleted)
            ((IAsyncResult)this).AsyncWaitHandle.WaitOne(3000);
        if (Error != null)
            throw cancelled ? new OperationCanceledException() : Error;
    }

    void MarkCompleted() {
        lock (_lockObject) {
            IsCompleted = true;
            this.waitHandle?.Set();
        }
    }

    public Exception Error { get; private set; }
    public bool IsCompleted { get; private set; }

    object IAsyncResult.AsyncState { get { return this.state; } }

    ManualResetEvent waitHandle;
    private readonly object _lockObject = new();

    WaitHandle IAsyncResult.AsyncWaitHandle {
        get {
            lock (_lockObject) {
                this.waitHandle ??= new ManualResetEvent(IsCompleted);
            }
            return this.waitHandle;
        }
    }

    bool IAsyncResult.CompletedSynchronously {
        get { return false; }
    }
}
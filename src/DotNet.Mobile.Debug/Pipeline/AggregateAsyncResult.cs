using System;
using System.Threading;

namespace DotNet.Mobile.Debug.Pipeline;

public class AggregateAsyncResult : IAsyncResult {
    readonly AsyncCallback callback;
    readonly object state;

    public Exception Error { get; private set; }
    public bool IsCompleted { get; private set; }

    private ManualResetEvent waitHandle;
    private readonly object _lockObject = new();

    object IAsyncResult.AsyncState { get { return this.state; } }

    WaitHandle IAsyncResult.AsyncWaitHandle {
        get {
            lock (this._lockObject) {
                this.waitHandle ??= new ManualResetEvent(IsCompleted);
            }
            return this.waitHandle;
        }
    }

    bool IAsyncResult.CompletedSynchronously {
        get { return false; }
    }

    public AggregateAsyncResult(AsyncCallback callback, object state) {
        this.callback = callback;
        this.state = state;
    }

    public void CompleteAsCallback(IAsyncResult ar) {
        Complete();
    }

    public void Complete() {
        MarkCompleted();
        this.callback?.Invoke(this);
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
        lock (this._lockObject) {
            IsCompleted = true;
            this.waitHandle?.Set();
        }
    }
}
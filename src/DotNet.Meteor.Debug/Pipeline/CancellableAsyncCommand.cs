using System;

namespace DotNet.Meteor.Debug.Pipeline;

public abstract class CancellableAsyncCommand : AggregateAsyncResult {
    private bool cancelled;
    private IAsyncResult innerResult;
    private readonly object _lockObject = new();

    protected CancellableAsyncCommand(AsyncCallback callback, object state) :
        base(callback, state) {
    }

    public bool CheckCancelled() {
        if (this.cancelled) {
            CompleteWithError(new OperationCanceledException());
        }
        return false;
    }

    public void SetInnerResult(IAsyncResult result) {
        lock (this._lockObject) {
            if (!this.cancelled) {
                this.innerResult = result;
                return;
            }
        }
        CancelInnerResult(result);
    }

    protected abstract void CancelInnerResult(IAsyncResult innerResult);

    public void Cancel() {
        IAsyncResult toCancel;
        lock (this._lockObject) {
            if (this.cancelled)
                return;
            this.cancelled = true;
            toCancel = this.innerResult;
            this.innerResult = null;
        }
        if (toCancel != null) {
            CancelInnerResult(toCancel);
        }
    }
}
namespace DotNet.Mobile.Debug.Events;

public class StoppedEvent : Event {
    public StoppedEvent(int tid, string reasn, string txt = null): base("stopped", new {
        threadId = tid,
        reason = reasn,
        text = txt
    }) { }
}
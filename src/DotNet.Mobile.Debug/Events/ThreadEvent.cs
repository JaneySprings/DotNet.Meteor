namespace DotNet.Mobile.Debug.Events;

public class ThreadEvent : Event {
    public ThreadEvent(string reasn, int tid): base("thread", new {
        reason = reasn,
        threadId = tid
    }) { }
}
namespace DotNet.Mobile.Debug.Events;

public class ExitedEvent : Event {
    public ExitedEvent(int exCode): base("exited", new { exitCode = exCode }) { }
}
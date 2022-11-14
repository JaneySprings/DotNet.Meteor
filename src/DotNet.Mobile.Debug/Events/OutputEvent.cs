namespace DotNet.Mobile.Debug.Events;

public class OutputEvent : Event {
    public OutputEvent(string cat, string outpt): base("output", new {
        category = cat,
        output = outpt
    }) { }
}
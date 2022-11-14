using System;

namespace DotNet.Mobile.Debug.Events;

public class ConsoleOutputEvent : Event {
    public ConsoleOutputEvent(string outpt): base("output", new {
        category = "console",
        output = outpt.Trim() + Environment.NewLine
    }) { }
}
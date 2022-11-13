using System.Text.Json.Serialization;
using System;

namespace DotNet.Mobile.Debug.Session;

public class Event : ProtocolMessage {
    [JsonPropertyName("event")] public string EventType { get; }
    [JsonPropertyName("body")] public dynamic Body { get; }

    public Event(string type, dynamic bdy = null) : base("event") {
        EventType = type;
        Body = bdy;
    }
}

public class StoppedEvent : Event {
    public StoppedEvent(int tid, string r, string t = null): base("stopped", new {
        threadId = tid,
        reason = r,
        text = t
    }) { }
}

public class TerminatedEvent : Event {
    public TerminatedEvent(): base("terminated") { }
}

public class ThreadEvent : Event {
    public ThreadEvent(string reasn, int tid): base("thread", new {
        reason = reasn,
        threadId = tid
    }) { }
}

public class OutputEvent : Event {
    public OutputEvent(string cat, string outpt): base("output", new {
        category = cat,
        output = outpt
    }) { }
}

public class InitializedEvent : Event {
    public InitializedEvent(): base("initialized") { }
}

public class ConsoleOutputEvent : Event {
    public ConsoleOutputEvent(string outpt): base("output", new {
        category = "console",
        output = outpt.Trim() + Environment.NewLine
    }) { }
}
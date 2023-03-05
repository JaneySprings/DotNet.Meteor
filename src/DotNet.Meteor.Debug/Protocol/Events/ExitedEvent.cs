using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol.Events;

/* The event indicates that debugging of the debuggee has terminated. 
* This does not mean that the debuggee itself has exited. */
public class ExitedEvent : Event {
    public ExitedEvent(int exitCode) : base("exited") {
        this.Body_ = new Body {
            ExitCode = exitCode
        };
    }

    private class Body {
        /* The exit code returned from the debugger. */
        [JsonPropertyName("exitCode")] public int ExitCode { get; set; }
    }
}
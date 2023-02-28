using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol.Events;

public class StoppedEvent : Event {
    public StoppedEvent(StoppedEvent.Body body) : base("stopped", body) {}
    public StoppedEvent(int tid, string reason, string text = null): base("stopped") {
        this.Body_ = new Body {
            ThreadId = tid,
            Reason = reason,
            Text = text
        };
    }

    public class Body {
        /* The reason for the event.
        * For backward compatibility this string is shown in the UI if the
        * `description` attribute is missing (but it must not be translated).
        * Values: 'step', 'breakpoint', 'exception', 'pause', 'entry', 'goto',
        * 'function breakpoint', 'data breakpoint', 'instruction breakpoint', etc. */
        [JsonPropertyName("reason")] public string Reason { get; set; }

        /* The thread which was stopped. */
        [JsonPropertyName("threadId")] public int ThreadId { get; set; }

        /* If `allThreadsStopped` is true, a debug adapter can announce that all
        * threads have stopped.
        * - The client should use this information to enable that all threads can
        * be expanded to access their stacktraces.
        * - If the attribute is missing or false, only the thread with the given
        * `threadId` can be expanded. */
        [JsonPropertyName("allThreadsStopped")] public bool AllThreadsStopped { get; set; }

        /* A value of true hints to the client that this event should not change the
        * focus. */
        [JsonPropertyName("preserveFocusHint")] public bool PreserveFocusHint { get; set; }

        /* Additional information. E.g. if reason is `exception`, text contains the
        * exception name. This string is shown in the UI. */
        [JsonPropertyName("text")] public string Text { get; set; }
    }
}
using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol.Events;

public class ThreadEvent: Event {
    public ThreadEvent(Body body) : base("thread", body) { }

    public class Body {
        /* The reason for the event.
        * Values: 'started', 'exited' */
        [JsonPropertyName("reason")] public string Reason { get; set; }

        /* The identifier of the thread. */
        [JsonPropertyName("threadId")] public int ThreadId { get; set; }

        public Body(string reason, int threadId) {
            this.Reason = reason;
            this.ThreadId = threadId;
        }
    }
}
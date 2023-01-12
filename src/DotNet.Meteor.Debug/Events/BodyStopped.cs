using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Events;

public class BodyStopped {
    [JsonPropertyName("reason")] public string Reason { get; set; }
    [JsonPropertyName("threadId")] public int ThreadId { get; set; }
    [JsonPropertyName("text")] public string Text { get; set; }

    public BodyStopped(int tid, string reason, string text = null) {
        this.ThreadId = tid;
        this.Reason = reason;
        this.Text = text;
    }
}
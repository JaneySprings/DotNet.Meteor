using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Events;

public class BodyThread {
    [JsonPropertyName("threadId")] public int ThreadId { get; set; }
    [JsonPropertyName("reason")] public string Reason { get; set; }

    public BodyThread(string reason, int tid) {
        this.Reason = reason;
        this.ThreadId = tid;
    }
}
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DotNet.Meteor.Debug.Protocol.Types;

namespace DotNet.Meteor.Debug.Protocol;

public class ThreadsResponseBody {
    [JsonPropertyName("threads")] public List<Thread> Threads { get; set; }

    public ThreadsResponseBody(List<Thread> threads) {
        this.Threads = threads;
    }
}
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Protocol;

public class BodyThreads {
    [JsonPropertyName("threads")] public List<ModelThread> Threads { get; set; }

    public BodyThreads(List<ModelThread> threads) {
        this.Threads = threads;
    }
}
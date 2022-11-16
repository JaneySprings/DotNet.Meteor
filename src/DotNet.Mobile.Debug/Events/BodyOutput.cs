using System;
using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Events;

public class BodyOutput {
    [JsonPropertyName("output")] public string Output { get; set; }
    [JsonPropertyName("category")] public string Category { get; set; }

    public BodyOutput(string category, string output) {
        this.Category = category;
        this.Output = output;
    }

    public BodyOutput(string output) {
        this.Category = "console";
        this.Output = output.Trim() + Environment.NewLine;
    }
}
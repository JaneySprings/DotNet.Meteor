using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Events;

public class BodyExited {
    [JsonPropertyName("exitCode")] public int ExitCode { get; set; }

    public BodyExited(int exitCode) {
        this.ExitCode = exitCode;
    }
}
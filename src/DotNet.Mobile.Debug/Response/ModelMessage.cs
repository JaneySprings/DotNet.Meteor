using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Protocol;

public class ModelMessage {
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("format")] public string Message { get; set; }

    public ModelMessage(int id, string message) {
        this.Id = id;
        this.Message = message;
    }
}
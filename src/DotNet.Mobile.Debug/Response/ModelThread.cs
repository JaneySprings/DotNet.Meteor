using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Protocol;

public class ModelThread {
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }

    public ModelThread(int id, string name) {
        this.Id = id;
        this.Name = string.IsNullOrEmpty(name) ? string.Format("Thread #{0}", id) : name;
    }
}
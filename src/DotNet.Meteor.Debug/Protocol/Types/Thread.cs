using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol.Types;

/* A Thread. */
public class Thread {
    /* Unique identifier for the thread. */
    [JsonPropertyName("id")] public int Id { get; set; }

    /* A name of the thread. */
    [JsonPropertyName("name")] public string Name { get; set; }

    public Thread(int id, string name) {
        this.Id = id;
        this.Name = string.IsNullOrEmpty(name) ? $"Thread #{id}" : name;
    }
}
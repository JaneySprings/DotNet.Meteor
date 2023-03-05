using System.Text.Json.Serialization;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug.Protocol;

public class ExceptionInfoResponseBody {
    /* ID of the exception that was thrown. */
    [JsonPropertyName("exceptionId")] public string ExceptionId { get; set; }

    /* Descriptive text for the exception. */
    [JsonPropertyName("description")] public string Description { get; set; }

    public ExceptionInfoResponseBody(ExceptionInfo exception) {
        this.ExceptionId = exception.Type;
        this.Description = exception.Message;
    }
}
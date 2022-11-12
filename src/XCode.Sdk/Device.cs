using System.Text.Json.Serialization;

namespace XCode.Sdk {
    public class Device {
        [JsonPropertyName("simulator")] public bool Simulator { get; set; }
        [JsonPropertyName("operatingSystemVersion")] public string OSVersion { get; set; }
        [JsonPropertyName("available")] public bool Available { get; set; }
        [JsonPropertyName("platform")] public string Platform { get; set; }
        [JsonPropertyName("modelCode")] public string ModelCode { get; set; }
        [JsonPropertyName("identifier")] public string Identifier { get; set; }
        [JsonPropertyName("architecture")] public string Architecture { get; set; }
        [JsonPropertyName("modelUTI")] public string ModelUTI { get; set; }
        [JsonPropertyName("model_name")] public string ModelName { get; set; }
        [JsonPropertyName("modelName")] public string Name { get; set; }
        [JsonPropertyName("interface")] public string Interface { get; set; }
        [JsonPropertyName("error")] public Error Error { get; set; }
    }

    public class Error {
        [JsonPropertyName("code")] public int Code { get; set; }
        [JsonPropertyName("failureReason")] public string FailureReason { get; set; }
        [JsonPropertyName("recoverySuggestion")] public string RecoverySuggestion { get; set; }
        [JsonPropertyName("description")] public string Description { get; set; }
    }
}
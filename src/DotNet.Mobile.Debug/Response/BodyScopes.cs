using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Protocol;

public class BodyScopes {
    [JsonPropertyName("scopes")] public List<ModelScope> Scopes { get; set; }

    public BodyScopes(List<ModelScope> scopes) {
        this.Scopes = scopes;
    }
}
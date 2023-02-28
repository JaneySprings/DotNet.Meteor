using System.Collections.Generic;
using System.Text.Json.Serialization;
using DotNet.Meteor.Debug.Protocol.Types;

namespace DotNet.Meteor.Debug.Protocol;

public class ScopesResponseBody {
    [JsonPropertyName("scopes")] public List<Scope> Scopes { get; set; }

    public ScopesResponseBody(List<Scope> scopes) {
        this.Scopes = scopes;
    }
}
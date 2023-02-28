using System.Collections.Generic;
using System.Text.Json.Serialization;
using DotNet.Meteor.Debug.Protocol.Types;

namespace DotNet.Meteor.Debug.Protocol;

public class VariablesResponseBody {
    [JsonPropertyName("variables")] public List<Variable> Variables { get; set; }

    public VariablesResponseBody(List<Variable> vars) {
        this.Variables = vars;
    }
}
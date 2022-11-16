using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotNet.Mobile.Debug.Protocol;

public class BodyVariables {
    [JsonPropertyName("variables")] public List<ModelVariable> Variables { get; set; }

    public BodyVariables(List<ModelVariable> vars) {
        this.Variables = vars;
    }
}
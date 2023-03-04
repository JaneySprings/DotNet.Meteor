using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol;

public class EvaluateResponseBody {
    /* The result of the evaluate request. */
    [JsonPropertyName("result")] public string Result { get; set; }

    /* If `variablesReference` is > 0, the evaluate result is structured and its
    * children can be retrieved by passing `variablesReference` to the
    * `variables` request as long as execution remains suspended. See 'Lifetime
    * of Object References' in the Overview section for details. */
    [JsonPropertyName("variablesReference")] public int VariablesReference { get; set; }

    public EvaluateResponseBody(string value, int reference = 0) {
        this.Result = value;
        this.VariablesReference = reference;
    }
}
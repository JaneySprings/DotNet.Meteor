using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol.Types;

/* A Variable is a name/value pair. */
public class Variable {
    /* The variable's name. */
    [JsonPropertyName("name")] public string Name { get; set; }

    /* The variable's value.
    * This can be a multi-line text, e.g. for a function the body of a function.
    * For structured variables (which do not have a simple value), it is
    * recommended to provide a one-line representation of the structured object.
    * This helps to identify the structured object in the collapsed state when
    * its children are not yet visible.
    * An empty string can be used if no value should be shown in the UI. */
    [JsonPropertyName("value")] public string Value { get; set; }

    /* The type of the variable's value.
    * Typically shown in the UI when hovering over the value. */
    [JsonPropertyName("type")] public string Type { get; set; }

    /* If `variablesReference` is > 0, the variable is structured and its children
    * can be retrieved by passing `variablesReference` to the `variables` request
    * as long as execution remains suspended. See 'Lifetime of Object References'
    * in the Overview section for details. */
    [JsonPropertyName("variablesReference")] public int VariablesReference { get; set; }

    public Variable(string name, string value, string type, int variablesReference = 0) {
        this.Name = name;
        this.Value = value;
        this.Type = type;
        this.VariablesReference = variablesReference;
    }
}
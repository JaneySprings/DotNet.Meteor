using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol.Types;

/* A Scope is a named container for variables. 
 * Optionally a scope can map to a source or a range within a source. */
public class Scope {
    /* Name of the scope such as 'Arguments', 'Locals', or 'Registers'. This
    * string is shown in the UI as is and can be translated. */
    [JsonPropertyName("name")] public string Name { get; set; }

    /* The variables of this scope can be retrieved by passing the value of
    * variablesReference to the VariablesRequest. */
    [JsonPropertyName("variablesReference")] public int VariablesReference { get; set; }

    /* A hint for how to present this scope in the UI. If this attribute is
    * missing, the scope is shown with a generic UI.
    * Values: 
    * 'arguments': Scope contains method arguments.
    * 'locals': Scope contains local variables.
    * 'registers': Scope contains registers. Only a single `registers` scope
    * should be returned from a `scopes` request. */
    [JsonPropertyName("presentationHint")] public string PresentationHint { get; set; }

    /* If true, the number of variables in this scope is large or expensive to retrieve. */
    [JsonPropertyName("expensive")] public bool Expensive { get; set; }

    public Scope(string name, int variablesReference, bool expensive = false, string presentationHint = null) {
        this.Name = name;
        this.VariablesReference = variablesReference;
        this.Expensive = expensive;
        this.PresentationHint = presentationHint;
    }
}
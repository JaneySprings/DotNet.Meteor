using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol.Types;

/* A Source is a descriptor for source code. */
public class Source {
    /* The short name of the source. Every source returned from the debug adapter
    * has a name.
    * When sending a source to the debug adapter this name is optional. */
    [JsonPropertyName("name")] public string Name { get; set; }

    /* The path of the source to be shown in the UI.
    * It is only used to locate and load the content of the source if no
    * `sourceReference` is specified (or its value is 0). */
    [JsonPropertyName("path")] public string Path { get; set; }

    /* If the value > 0 the contents of the source must be retrieved through the
    * `source` request (even if a path is specified).
    * Since a `sourceReference` is only valid for a session, it can not be used
    * to persist a source.
    * The value should be less than or equal to 2147483647 (2^31-1). */
    [JsonPropertyName("sourceReference")] public int SourceReference { get; set; }

    /* A hint for how to present the source in the UI.
    * A value of `deemphasize` can be used to indicate that the source is not
    * available or that it is skipped on stepping.
    * Values: 'normal', 'emphasize', 'deemphasize'
    */
    [JsonPropertyName("presentationHint")] public string PresentationHint { get; set; }

    /* The origin of this source. For example, 'internal module', 'inlined content
    * from source map', etc. */
    [JsonPropertyName("origin")] public string Origin { get; set; }

    public Source() { }
    public Source(string name, string path, int sourceReference, string hint) {
        this.Name = name;
        this.Path = path;
        this.SourceReference = sourceReference;
        this.PresentationHint = hint;
    }
}
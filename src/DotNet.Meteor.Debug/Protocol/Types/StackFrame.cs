using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol.Types;

/* A Stackframe contains the source location. */
public class StackFrame {
    /* An identifier for the stack frame. It must be unique across all threads. */
    [JsonPropertyName("id")] public int Id { get; set; }

    /* The source of the frame. */
    [JsonPropertyName("source")] public Source Source { get; set; }

    /* The line within the file of the frame. If source is null or doesn't exist,
    * line is 0 and must be ignored. */
    [JsonPropertyName("line")] public int Line { get; set; }

    /* The column within the line. If source is null or doesn't exist, column is 0
    * and must be ignored. */
    [JsonPropertyName("column")] public int Column { get; set; }

    /* The end line of the range covered by the stack frame. */
    [JsonPropertyName("endLine")] public int EndLine { get; set; }

    /* End position of the range covered by the stack frame. It is measured in
    * UTF-16 code units and the client capability `columnsStartAt1` determines
    * whether it is 0- or 1-based. */
    [JsonPropertyName("endColumn")] public int EndColumn { get; set; }

    /* The name of the stack frame, typically a method name. */
    [JsonPropertyName("name")] public string Name { get; set; }

    /* An optional hint for how to present this frame in the UI. A value of
    * 'label' can be used to indicate that the frame is an artificial frame that
    * is used as a visual label or separator. A value of 'subtle' can be used to
    * change the appearance of a frame in a 'subtle' way.
    * Values: 'normal', 'label', 'subtle'. */
    [JsonPropertyName("presentationHint")] public string PresentationHint { get; set; }

    public StackFrame(int id, Source source, string hint, string name, int line, int column, int endLine, int endColumn) {
        this.Id = id;
        this.Name = name;
        this.Source = source;

        // These should NEVER be negative
        this.Line = System.Math.Max(0, line);
        this.Column = System.Math.Max(0, column);
        this.EndLine = System.Math.Max(0, endLine);
        this.EndColumn = System.Math.Max(0, endColumn);

        this.PresentationHint = hint;
    }
}
namespace DotNet.Mobile.Debug.Entities;

public class StackFrame {
    public int id { get; }
    public Source source { get; }
    public int line { get; }
    public int column { get; }
    public string name { get; }
    public string presentationHint { get; }

    public StackFrame(int id, string name, Source source, int line, int column, string hint) {
        this.id = id;
        this.name = name;
        this.source = source;

        // These should NEVER be negative
        this.line = System.Math.Max(0, line);
        this.column = System.Math.Max(0, column);

        presentationHint = hint;
    }
}
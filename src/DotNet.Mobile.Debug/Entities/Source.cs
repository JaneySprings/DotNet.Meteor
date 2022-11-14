namespace DotNet.Mobile.Debug.Entities;

public class Source {
    public string name { get; }
    public string path { get; }
    public int sourceReference { get; }
    public string presentationHint { get; }

    public Source(string name, string path, int sourceReference, string hint) {
        this.name = name;
        this.path = path;
        this.sourceReference = sourceReference;
        this.presentationHint = hint;
    }
}
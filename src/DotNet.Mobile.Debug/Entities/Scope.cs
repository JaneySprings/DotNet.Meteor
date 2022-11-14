namespace DotNet.Mobile.Debug.Entities;

public class Scope {
    public string name { get; }
    public int variablesReference { get; }
    public bool expensive { get; }

    public Scope(string name, int variablesReference, bool expensive = false) {
        this.name = name;
        this.variablesReference = variablesReference;
        this.expensive = expensive;
    }
}
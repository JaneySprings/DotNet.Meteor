namespace DotNet.Mobile.Debug.Entities;

public class Variable {
    public string name { get; }
    public string value { get; }
    public string type { get; }
    public int variablesReference { get; }

    public Variable(string name, string value, string type, int variablesReference = 0) {
        this.name = name;
        this.value = value;
        this.type = type;
        this.variablesReference = variablesReference;
    }
}
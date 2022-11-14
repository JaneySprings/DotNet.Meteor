namespace DotNet.Mobile.Debug.Protocol;

public class EvaluateResponseBody : ResponseBody {
    public string result { get; }
    public int variablesReference { get; }

    public EvaluateResponseBody(string value, int reff = 0) {
        result = value;
        variablesReference = reff;
    }
}
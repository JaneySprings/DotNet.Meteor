namespace DotNet.Mobile.Debug.Session;

public class Message {
    public int id { get; }
    public string format { get; }
    public dynamic variables { get; }
    public dynamic showUser { get; }
    public dynamic sendTelemetry { get; }

    public Message(int id, string format, dynamic variables = null, bool user = true, bool telemetry = false) {
        this.id = id;
        this.format = format;
        this.variables = variables;
        showUser = user;
        sendTelemetry = telemetry;
    }
}
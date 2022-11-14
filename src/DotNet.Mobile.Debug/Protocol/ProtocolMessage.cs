namespace DotNet.Mobile.Debug.Protocol;

public class ProtocolMessage {
    public int seq;
    public string type;

    public ProtocolMessage() {}
    public ProtocolMessage(string typ) {
        this.type = typ;
    }
    public ProtocolMessage(string typ, int sq) {
        this.type = typ;
        this.seq = sq;
    }
}
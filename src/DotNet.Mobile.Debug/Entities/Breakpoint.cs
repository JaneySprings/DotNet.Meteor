namespace DotNet.Mobile.Debug.Entities;

public class Breakpoint {
    public bool verified { get; }
    public int line { get; }

    public Breakpoint(bool verified, int line) {
        this.verified = verified;
        this.line = line;
    }
}
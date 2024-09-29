using DotNet.Meteor.Common.Android;

namespace DotNet.Meteor.HotReload.Providers;

public class AndroidConnectionProvider : UniversalConnectionProvider {
    private string serial;

    public AndroidConnectionProvider(int port, string serial) : base(port) {
        this.serial = serial;
    }

    public new Task<bool> PrepareTransportAsync() {
        AndroidDebugBridge.Forward(serial, Port);
        return Task.FromResult(true);
    }
}
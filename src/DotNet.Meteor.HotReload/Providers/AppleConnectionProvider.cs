using DotNet.Meteor.Common.Apple;

namespace DotNet.Meteor.HotReload.Providers;

public class AppleConnectionProvider : UniversalConnectionProvider {
    private string serial;

    public AppleConnectionProvider(int port, string serial) : base(port) {
        this.serial = serial;
    }

    public new Task<bool> PrepareTransportAsync() {
        _ = MonoLauncher.TcpTunnel(serial, Port);
        return Task.FromResult(true);
    }
}
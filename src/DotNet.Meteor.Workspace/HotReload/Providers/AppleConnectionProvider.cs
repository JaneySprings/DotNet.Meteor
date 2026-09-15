using DotNet.Debugging.Common.Apple;
using DotNet.Meteor.Workspace.HotReload;

namespace DotNet.Meteor.HotReload.Providers;

public class AppleConnectionProvider : UniversalConnectionProvider {
    private string serial;

    public AppleConnectionProvider(int port, string serial) : base(port) {
        this.serial = serial;
    }

    public new Task<bool> PrepareTransportAsync() {
        _ = MonoLauncher.TcpTunnel(serial, new[] { Port });
        return Task.FromResult(true);
    }
}
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNet.Meteor.HotReloadPlugin;

public static class BuilderExtensions {
    public static MauiAppBuilder EnableHotReload(this MauiAppBuilder builder, int idePort = 9988) {
        builder.Services.TryAdd(ServiceDescriptor.Transient<IMauiInitializeService, Server>(_ => new Server(idePort)));
        return builder;
    }
}

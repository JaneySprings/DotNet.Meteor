using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Esp.Resources;
using System.Reflection;
using Microsoft.Maui.Hosting;

[assembly: AssemblyMetadata("IsTrimmable", "True")]
namespace DotNet.Mobile.HotReload;

public static class Extension {
	public static MauiAppBuilder EnableHotReload(this MauiAppBuilder builder, string? ideIp = null, int idePort = Constants.DEFAULT_PORT) {
		builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService, HotReloadBuilder>(_ => new HotReloadBuilder {
			IdeIp = ideIp,
			IdePort = idePort,
		}));
		return builder;
	}
}

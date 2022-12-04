using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.HotReload;
using Microsoft.Maui.Hosting;
using Microsoft.Maui;

namespace DotNet.Mobile.HotReload;

public class HotReloadBuilder : IMauiInitializeService {
    public string? IdeIp { get; set; }
    public int IdePort { get; set; } = 9988;

    public async void Initialize(IServiceProvider services) {
        var handlers = services.GetRequiredService<IMauiHandlersFactory>();

        MauiHotReloadHelper.RegisterHandlers(handlers.GetCollection());

        Reloadify.Reload.Instance.ReplaceType = (d) => {
            MauiHotReloadHelper.RegisterReplacedView(d.ClassName, d.Type);
        };
        Reloadify.Reload.Instance.FinishedReload = () => {
            MauiHotReloadHelper.TriggerReload();
        };

        await Task.Run(async () => {
            try {
                var success = await Reloadify.Reload.Init(IdeIp, IdePort);

                Console.WriteLine($"HotReload Initialize: {success}");
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
        });
    }
}
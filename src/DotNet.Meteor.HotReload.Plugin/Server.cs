using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.Json;
using DotNet.Meteor.HotReload.Plugin.Models;

namespace DotNet.Meteor.HotReload.Plugin;

internal class Server : IMauiInitializeService {
    private const string Version = "1.0.0";
    private int idePort;

    internal Server(int port) {
        idePort = port;
    }

    public async void Initialize(IServiceProvider services) {
        var tcpListener = new TcpListener(IPAddress.Loopback, idePort);
        try {
            tcpListener.Start();
        } catch (Exception e) {
            Logger.LogError(e);
            return;
        }

        using var client = await tcpListener.AcceptTcpClientAsync();
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream) { AutoFlush = true };

        while (true) {
            await reader.ReadLineAsync();
            await writer.WriteLineAsync($"handshake_{Version}");

            var response = await reader.ReadLineAsync();
            var transferObject = JsonSerializer.Deserialize<TransferObject>(response, TrimmableContext.Default.TransferObject);
            var mainPage = Application.Current?.MainPage;

            if (mainPage == null || transferObject == null)
                continue;

            // Modal pages are on top of the navigation stack
            if (mainPage.Navigation.ModalStack.Count > 0) {
                var modalPage = mainPage.Navigation.ModalStack.Last();
                if (modalPage != null) {
                    TraverseVisualTree(modalPage, transferObject);
                    continue;
                }
            }
            // Pages are on the navigation stack
            if (mainPage.Navigation.NavigationStack.Count > 0) {
                var page = mainPage.Navigation.NavigationStack.Last();
                if (page != null) {
                    TraverseVisualTree(page, transferObject);
                    continue;
                }
            }
            // Fall back to the main page
            TraverseVisualTree(mainPage, transferObject);
        }
    }

    private void TraverseVisualTree(Element node, TransferObject transferObject) {
#pragma warning disable CS0618 // Used by hot reload
        foreach (Element child in node.LogicalChildren)
            TraverseVisualTree(child, transferObject);
#pragma warning restore CS0618

        if (node.GetType().FullName == transferObject.Definition) {
            ReloadElement(node, transferObject);
            return;
        }

        return;
    }
    private void ReloadElement(Element element, TransferObject transferObject) {
        if (element is VisualElement visualElement)
            visualElement.Resources.Clear();
        if (element is Page page)
            page.ToolbarItems.Clear();

        try {
            element.LoadFromXaml(transferObject.Content);
            if (transferObject.Transformations == null)
                return;

            var fields = element.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var transformation in transferObject.Transformations) {
                var destination = fields.FirstOrDefault(f => f.Name == transformation.Key);
                var newValue = element.FindByName(transformation.Value);
                if (destination == null || newValue == null)
                    continue;

                destination.SetValue(element, newValue);
            }
        } catch (Exception e) {
            Logger.LogError(e);
        }
    }
}
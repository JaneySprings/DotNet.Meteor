using System.Net;
using System.Net.Sockets;

namespace DotNet.Meteor.HotReload.Plugin;

internal class Server : IMauiInitializeService {
    public int IdePort { get; }

    internal Server(int port) {
        IdePort = port;
    }

    public async void Initialize(IServiceProvider services) {
        var tcpListener = new TcpListener(IPAddress.Loopback, IdePort);
        try {
            tcpListener.Start();
        } catch (Exception e) {
            Logger.LogError(e);
            return;
        }

        while (true) {
            var client = await tcpListener.AcceptTcpClientAsync();
            var stream = client.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream) { AutoFlush = true };

            // wait for empty message
            await reader.ReadLineAsync();
            // send handshake
            await writer.WriteLineAsync("handshake");

            var classDefinition = await reader.ReadLineAsync();
            var xamlContent = await reader.ReadToEndAsync();
            var mainPage = Application.Current?.MainPage;
            
            if (mainPage == null || string.IsNullOrEmpty(classDefinition) || string.IsNullOrEmpty(xamlContent))
                continue;

            // Modal pages are on top of the navigation stack
            if (mainPage.Navigation.ModalStack.Count > 0) {
                var modalPage = mainPage.Navigation.ModalStack.Last();
                if (modalPage != null) {
                    TraverseVisualTree(modalPage, classDefinition, xamlContent);
                    continue;
                }
            }
            // Pages are on the navigation stack
            if (mainPage.Navigation.NavigationStack.Count > 0) {
                var page = mainPage.Navigation.NavigationStack.Last();
                if (page != null) {
                    TraverseVisualTree(page, classDefinition, xamlContent);
                    continue;
                }
            }
            // Fall back to the main page
            TraverseVisualTree(mainPage, classDefinition, xamlContent);
        }
    }

    private void ReloadElement(Element element, string xamlContent) {
        if (element is VisualElement visualElement)
            visualElement.Resources.Clear();
        if (element is Page page)
            page.ToolbarItems.Clear();

        try {
            element.LoadFromXaml(xamlContent);
        } catch (Exception e) {
            Logger.LogError(e);
        }
    }

#pragma warning disable CS0618 // Used by hot reload
    private void TraverseVisualTree(Element node, string xClass, string xamlContent) {
        foreach (Element child in node.LogicalChildren)
            TraverseVisualTree(child, xClass, xamlContent);

        if (node.GetType().FullName == xClass) {
            ReloadElement(node, xamlContent);
            return;
        }

        return;
    }
#pragma warning restore CS0618
}
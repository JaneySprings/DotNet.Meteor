using System.Net;
using System.Net.Sockets;

namespace DotNet.Meteor.HotReloadPlugin;

internal class Server : IMauiInitializeService {
    public int IdePort { get; }

    internal Server(int port) {
        IdePort = port;
    }

    public async void Initialize(IServiceProvider services) {
        var tcpListener = new TcpListener(IPAddress.Loopback, IdePort);
        tcpListener.Start();

        while (true) {
            var client = await tcpListener.AcceptTcpClientAsync();
            var stream = client.GetStream();
            var reader = new StreamReader(stream);

            var classDefinition = await reader.ReadLineAsync();
            var xamlContent = await reader.ReadToEndAsync();
            
            if (Application.Current.MainPage == null)
                continue;

            var targetElement = GetElement(Application.Current.MainPage, classDefinition);
            if (targetElement == null)
                continue;

            targetElement.LoadFromXaml(xamlContent);
        }
    }

#pragma warning disable CS0618 // Used by hot reload
    private Element GetElement(Element node, string xClass) {
        if (node is not Element) 
            return null;
        if (node.GetType().FullName == xClass) 
            return node;
        if (node.LogicalChildren.Count == 0) 
            return null;
    
        foreach (Element child in node.LogicalChildren) {
            Element result = GetElement(child, xClass);
            if (result is not null) 
                return result;
        }

        return null;
    }
#pragma warning restore CS0618
}
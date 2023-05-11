using System.Net.Sockets;
using System.Text;

namespace DotNet.Meteor.HotReload;

public static class HotReloadClient {
    public static void SendNotification(int port, string xamlFilePath, Action<string>? logger = null) {
        if (!File.Exists(xamlFilePath)) {
            logger?.Invoke($"XAML file not found: {xamlFilePath}");
            throw new FileNotFoundException("XAML file not found", xamlFilePath);
        }

        var xamlContent = new StringBuilder(File.ReadAllText(xamlFilePath));
        var classDefinition = MarkupHelper.GetClassDefinition(xamlContent.ToString());
        MarkupHelper.RemoveReferenceNames(xamlContent);

        if (string.IsNullOrEmpty(classDefinition)) {
            logger?.Invoke($"Class definition not found in XAML file: {xamlFilePath}");
            throw new Exception($"Class definition not found in XAML file: {xamlFilePath}");
        }

        using var client = new TcpClient("localhost", port);
        using var stream = client.GetStream();
        using var writer = new StreamWriter(stream);
        writer.WriteLine(classDefinition);
        writer.WriteLine(xamlContent);
        writer.Flush();
    }
}
using System.Net.Sockets;
using System.Text;

namespace DotNet.Meteor.HotReload;

public static class HotReloadClient {
    public static bool SendNotification(int port, string xamlFilePath, Action<string>? logger = null) {
        if (!File.Exists(xamlFilePath)) {
            logger?.Invoke($"XAML file not found: {xamlFilePath}");
            return false;
        }

        var xamlContent = new StringBuilder(File.ReadAllText(xamlFilePath));
        var classDefinition = MarkupHelper.GetClassDefinition(xamlContent);
        MarkupHelper.ModifyReferenceNames(xamlContent);

        if (string.IsNullOrEmpty(classDefinition)) {
            logger?.Invoke($"Class definition not found in XAML file: {xamlFilePath}");
            return false;
        }

        try {
            using var client = new TcpClient("localhost", port);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            using var reader = new StreamReader(stream);

            // Send 'handshake' message to server
            writer.WriteLine("handshake");
            var response = reader.ReadLine();
            if (response != "handshake") {
                logger?.Invoke($"Server responded with unexpected message: {response}");
                return false;
            }

            // Send notification to server if 'handshake' was successful
            writer.WriteLine(classDefinition);
            writer.Write(xamlContent);

        } catch (Exception ex) {
            logger?.Invoke($"Error sending notification to Hot Reload server: {ex.Message}");
            return false;
        }

        return true;
    }
}
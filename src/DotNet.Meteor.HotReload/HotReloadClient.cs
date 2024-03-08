using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DotNet.Meteor.HotReload.Extensions;
using DotNet.Meteor.HotReload.Models;

namespace DotNet.Meteor.HotReload;

public static class HotReloadClient {
    public static bool SendNotification(int port, string xamlFilePath, Action<string>? logger = null) {
        if (!File.Exists(xamlFilePath)) {
            logger?.Invoke($"XAML file not found: {xamlFilePath}");
            return false;
        }

        var xamlContent = new StringBuilder(File.ReadAllText(xamlFilePath));
        var classDefinition = MarkupExtensions.GetClassDefinition(xamlContent);
        if (string.IsNullOrEmpty(classDefinition)) {
            logger?.Invoke($"Class definition not found in XAML file: {xamlFilePath}");
            return false;
        }

        var transformations = MarkupExtensions.TransformReferenceNames(xamlContent);
        var transferObject = new TransferObject {
            Version = Program.GetVersion(),
            Content = xamlContent.ToString(),
            Definition = classDefinition,
            Transformations = transformations
        };

        try {
            using var client = new TcpClient("localhost", port);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            using var reader = new StreamReader(stream);

            writer.WriteLine(HotReloadProtocol.HandShakeKey);
            
            var protocol = new HotReloadProtocol();
            protocol.CheckServerCapabilities(reader.ReadLine());

            if (!protocol.IsConnectionSuccessful) {
                logger?.Invoke($"Server responded with unexpected message");
                return false;
            }

            if (!protocol.IsLegacyProtocolFormat) {
                writer.Write(JsonSerializer.Serialize(transferObject, TrimmableContext.Default.TransferObject));  
            } else {
                logger?.Invoke($"Server is using legacy protocol format");
                writer.WriteLine(transferObject.Definition);
                writer.Write(transferObject.Content);
            }
        } catch (Exception ex) {
            logger?.Invoke($"Error sending notification to Hot Reload server: {ex.Message}");
            return false;
        }

        return true;
    }
}
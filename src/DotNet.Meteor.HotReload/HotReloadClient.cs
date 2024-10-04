using System.Reflection;
using System.Text;
using System.Text.Json;
using DotNet.Meteor.Common.Processes;
using DotNet.Meteor.HotReload.Extensions;
using DotNet.Meteor.HotReload.Models;
using DotNet.Meteor.HotReload.Providers;

namespace DotNet.Meteor.HotReload;

public class HotReloadClient : IDisposable {
    private IConnectionProvider connectionProvider;
    private IProcessLogger? logger;

    public bool IsRunning => connectionProvider.TransportReader != null && connectionProvider.TransportWriter != null;

    public HotReloadClient(IConnectionProvider connectionProvider, IProcessLogger? logger = null) {
        this.connectionProvider = connectionProvider;
        this.logger = logger;
    }

    public Task<bool> PrepareTransportAsync() {
        return connectionProvider.PrepareTransportAsync();
    }
    public async Task<bool> SendNotificationAsync(string? filePath) {
        try {
            if (!IsRunning)
                await connectionProvider.TryConnectAsync();

            if (!File.Exists(filePath)) {
                logger?.OnErrorDataReceived($"[HotReload]: XAML file not found: {filePath}");
                return false;
            }
            if (connectionProvider.TransportReader == null || connectionProvider.TransportWriter == null) {
                logger?.OnErrorDataReceived($"[HotReload]: Connection to server is not established");
                return false;
            }

            var xamlContent = new StringBuilder(File.ReadAllText(filePath));
            var classDefinition = MarkupExtensions.GetClassDefinition(xamlContent);
            if (string.IsNullOrEmpty(classDefinition)) {
                logger?.OnErrorDataReceived($"[HotReload]: Class definition not found in XAML file: {filePath}");
                return false;
            }

            var transformations = MarkupExtensions.TransformReferenceNames(xamlContent);
            var transferObject = new TransferObject {
                Version = GetVersion(),
                Content = xamlContent.ToString(),
                Definition = classDefinition,
                Transformations = transformations
            };

            connectionProvider.TransportWriter.WriteLine(HotReloadProtocol.HandShakeKey);

            var protocol = new HotReloadProtocol();
            protocol.CheckServerCapabilities(connectionProvider.TransportReader.ReadLine());

            if (!protocol.IsConnectionSuccessful) {
                logger?.OnErrorDataReceived("[HotReload]: Server responded with unexpected message");
                return false;
            }
            if (protocol.IsLegacyProtocolFormat) {
                logger?.OnErrorDataReceived("[HotReload]: Server used legacy protocol format");
                return false;
            }

            connectionProvider.TransportWriter.WriteLine(JsonSerializer.Serialize(transferObject, TrimmableContext.Default.TransferObject));
            return true;
        } catch (Exception ex) {
            logger?.OnErrorDataReceived($"[HotReload]: {ex.Message}");
            return false;
        }
    }

    private static string GetVersion() {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version?.ToString() ?? "1.0.0";
    }
    public void Dispose() {
        connectionProvider.TransportReader?.Dispose();
        connectionProvider.TransportWriter?.Dispose();
    }
}
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using DotNet.Meteor.HotReload.Extensions;
using DotNet.Meteor.HotReload.Models;
using DotNet.Meteor.Processes;

namespace DotNet.Meteor.HotReload;

public class HotReloadClient {
    private StreamReader? transportReader;
    private StreamWriter? transportWriter;
    private readonly int maxConnectionAttempts;
    private readonly int timeBetweenConnectionAttempts;
    
    public bool IsRunning => transportReader != null && transportWriter != null;
    public bool IsSupported { get; private set; }

    public HotReloadClient(int maxConnectionAttempts = 60, int timeBetweenConnectionAttempts = 1000) {
        this.maxConnectionAttempts = maxConnectionAttempts;
        this.timeBetweenConnectionAttempts = timeBetweenConnectionAttempts;
    }

    public async Task<bool> TryConnectAsync(int port) {
        IsSupported = true;
        for (var i = 0; i < maxConnectionAttempts; i++) {
            try {
                var client = new TcpClient("localhost", port);
                var stream = client.GetStream();
                transportReader = new StreamReader(stream);
                transportWriter = new StreamWriter(stream) { AutoFlush = true };
                return true;
            } catch {
                await Task.Delay(timeBetweenConnectionAttempts);
            }
        }
        return false;
    }
    public void SendNotification(string filePath, IProcessLogger? logger = null) {
        if (!File.Exists(filePath)) {
            logger?.OnErrorDataReceived($"[HotReload]: XAML file not found: {filePath}");
            return;
        }
        if (transportReader == null || transportWriter == null) {
            logger?.OnErrorDataReceived($"[HotReload]: Connection not established");
            return;
        }

        var xamlContent = new StringBuilder(File.ReadAllText(filePath));
        var classDefinition = MarkupExtensions.GetClassDefinition(xamlContent);
        if (string.IsNullOrEmpty(classDefinition)) {
            logger?.OnErrorDataReceived($"[HotReload]: Class definition not found in XAML file: {filePath}");
            return;
        }

        var transformations = MarkupExtensions.TransformReferenceNames(xamlContent);
        var transferObject = new TransferObject {
            Version = GetVersion(),
            Content = xamlContent.ToString(),
            Definition = classDefinition,
            Transformations = transformations
        };

        transportWriter.WriteLine(HotReloadProtocol.HandShakeKey);

        var protocol = new HotReloadProtocol();
        protocol.CheckServerCapabilities(transportReader.ReadLine());

        if (!protocol.IsConnectionSuccessful) {
            logger?.OnErrorDataReceived("[HotReload]: Server responded with unexpected message");
            return;
        }
        if (protocol.IsLegacyProtocolFormat) {
            logger?.OnErrorDataReceived("[HotReload]: Server used legacy protocol format");
            return;
        }

        transportWriter.WriteLine(JsonSerializer.Serialize(transferObject, TrimmableContext.Default.TransferObject));
    }
    public void Close() {
        transportReader?.Close();
        transportWriter?.Close();
        transportReader = null;
        transportWriter = null;
    }

    private static string GetVersion() {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version?.ToString() ?? "1.0.0";
    }
}
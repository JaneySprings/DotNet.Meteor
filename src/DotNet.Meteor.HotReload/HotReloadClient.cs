using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using DotNet.Meteor.HotReload.Extensions;
using DotNet.Meteor.HotReload.Models;
using DotNet.Meteor.Processes;

namespace DotNet.Meteor.HotReload;

public class HotReloadClient {
    private const int MaxConnectionAttempts = 60;
    private const int TimeBetweenConnectionAttempts = 1000;

    private StreamReader? transportReader;
    private StreamWriter? transportWriter;
    private readonly bool lastConnectionAttemptAllowed; // only for f**king android
    private readonly int port;

    public bool IsRunning => transportReader != null && transportWriter != null;

    public HotReloadClient(int port, bool lastConnectionAttemptAllowed = false) {
        this.lastConnectionAttemptAllowed = lastConnectionAttemptAllowed;
        this.port = port;
    }

    public async Task<bool> TryConnectAsync() {
        if (IsRunning)
            return true;
        if (lastConnectionAttemptAllowed)
            return false;

        return await TryConnectCoreAsync();
    }
    public void SendNotification(string filePath, IProcessLogger? logger = null) {
        if (lastConnectionAttemptAllowed && !IsRunning)
            TryConnectCoreAsync().Wait();

        if (!File.Exists(filePath)) {
            logger?.OnErrorDataReceived($"[HotReload]: XAML file not found: {filePath}");
            return;
        }
        if (transportReader == null || transportWriter == null) {
            logger?.OnErrorDataReceived($"[HotReload]: Connection to server is not established");
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

    private async Task<bool> TryConnectCoreAsync() {
        var maxConnectionAttempts = lastConnectionAttemptAllowed ? 1 : MaxConnectionAttempts;
        var timeBetweenConnectionAttempts = lastConnectionAttemptAllowed ? 1 : TimeBetweenConnectionAttempts;

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
    private static string GetVersion() {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version?.ToString() ?? "1.0.0";
    }
}
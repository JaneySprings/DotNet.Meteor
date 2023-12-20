using System;
using System.IO;
using System.Net.Sockets;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug.Extensions;

internal class ExternalTypeResolver {
    private readonly DebuggerSessionOptions sessionOptions;
    private readonly string resolvedConnectionInfoPath;
    private readonly TcpClient client;

    public ExternalTypeResolver(string tempFolder, DebuggerSessionOptions options) {
        sessionOptions = options;
        resolvedConnectionInfoPath = Path.Combine(tempFolder, "resolve.drc");
        client = new TcpClient();
       
        if (!Directory.Exists(tempFolder))
            Directory.CreateDirectory(tempFolder);
        if (options.EvaluationOptions.UseExternalTypeResolver)
            File.WriteAllText(resolvedConnectionInfoPath, string.Empty);
    }

    public string Handle(string typeName, SourceLocation location) {
        try {
            EnsureConnected();
            // Not use using to avoid closing the stream
            var stream = client.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream) { AutoFlush = true };

            writer.WriteLine("ResolveType");
            writer.WriteLine($"{location.FileName}|{location.Line}|{typeName}");

            var request = reader.ReadLineAsync();
            request.Wait(sessionOptions.EvaluationOptions.EvaluationTimeout);
            return request.IsCompleted ? request.Result : typeName;
        } 
        catch (Exception) {
            return typeName;
        }
    }
    public void Dispose() {
        client?.Close();
        if (File.Exists(resolvedConnectionInfoPath))
            File.Delete(resolvedConnectionInfoPath);
    }

    private bool EnsureConnected() {
        if (client.Connected)
            return true;
        if (!File.Exists(resolvedConnectionInfoPath))
            return false;

        var content = File.ReadAllText(resolvedConnectionInfoPath);
        if (string.IsNullOrEmpty(content) || !int.TryParse(content, out var port))
            return false;

        client.Connect("localhost", port);
        return client.Connected;
    }
}
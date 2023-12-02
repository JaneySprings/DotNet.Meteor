using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug.Extensions;

internal class ExternalTypeResolver {
    private readonly DebuggerSessionOptions sessionOptions;
    private readonly string resolvedConnectionInfoPath;

    public ExternalTypeResolver(string projectFile, DebuggerSessionOptions options) {
        sessionOptions = options;
        resolvedConnectionInfoPath = Path.Combine(Path.GetDirectoryName(projectFile), ".meteor", "resolve.drc");
        if (options.EvaluationOptions.UseExternalTypeResolver)
            File.WriteAllText(resolvedConnectionInfoPath, string.Empty);
    }

    public string Handle(string typeName, SourceLocation location) {
        if (!sessionOptions.EvaluationOptions.UseExternalTypeResolver)
            return typeName;
        if (!File.Exists(resolvedConnectionInfoPath))
            return typeName;

        var port = 0;
        var content = File.ReadAllText(resolvedConnectionInfoPath);
        if (string.IsNullOrEmpty(content) || !int.TryParse(content, out port))
            return typeName;
        
        try {
            using var client = new TcpClient("localhost", port);
            using var stream = client.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream) { AutoFlush = true };

            writer.WriteLine("ResolveType");
            writer.WriteLine($"{location.FileName}|{location.Line}|{typeName}");

            var request = reader.ReadLineAsync();
            request.Wait(sessionOptions.EvaluationOptions.EvaluationTimeout);
            return request.Result;
        } catch (Exception) {
            return typeName;
        }
    }

    public void Dispose() {
        if (File.Exists(resolvedConnectionInfoPath))
            File.Delete(resolvedConnectionInfoPath);
    }
}
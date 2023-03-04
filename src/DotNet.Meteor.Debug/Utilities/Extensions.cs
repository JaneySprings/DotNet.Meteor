using System;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;

namespace DotNet.Meteor.Debug.Utilities;

public static class Extensions {
    private readonly static string[] MonoExtensions = new String[] {
        ".cs", ".csx", ".fs", ".fsi", ".ml", ".mli", ".fsx", ".fsscript", ".hx", ".vb", ".razor"
    };

    public static byte[] ConvertToBytes(this object obj) {
        var encoding = Encoding.UTF8;
        var asJson = JsonSerializer.Serialize(obj);
        byte[] jsonBytes = encoding.GetBytes(asJson);

        string header = string.Format("Content-Length: {0}{1}", jsonBytes.Length, "\r\n\r\n");
        byte[] headerBytes = encoding.GetBytes(header);

        byte[] data = new byte[headerBytes.Length + jsonBytes.Length];
        Buffer.BlockCopy(headerBytes, 0, data, 0, headerBytes.Length);
        Buffer.BlockCopy(jsonBytes, 0, data, headerBytes.Length, jsonBytes.Length);

        return data;
    }

    public static bool HasMonoExtension(this string path) {
        if (string.IsNullOrEmpty(path))
            return false;
        foreach (var ext in MonoExtensions) {
            if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    public static int FindFreePort() {
        TcpListener listener = null;
        try {
            listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        } finally {
            listener.Stop();
        }
    }
}

public static class UriPathConverter {
    public static string DebuggerPathToClient(string path) {
        try {
            var uri = new Uri(path);
            return uri.AbsoluteUri;
        } catch {
            return null;
        }
    }

    public static string ClientPathToDebugger(string path) {
        if (string.IsNullOrEmpty(path))
            return null;
        if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
            return new Uri(path).LocalPath;
        return null;
    }
}
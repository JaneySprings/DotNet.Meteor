using System;
using System.Text;
using System.Text.Json;

namespace DotNet.Meteor.Debug;

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

    public static string ConvertDebuggerPathToClient(this string path, bool clientPathsAreURI) {
        if (clientPathsAreURI) {
            try {
                var uri = new Uri(path);
                return uri.AbsoluteUri;
            } catch {
                return null;
            }
        } else {
            return path;
        }
    }

    public static string ConvertClientPathToDebugger(this string clientPath, bool clientPathsAreURI) {
        if (clientPath == null)
            return null;

        if (clientPathsAreURI) {
            if (Uri.IsWellFormedUriString(clientPath, UriKind.Absolute)) {
                Uri uri = new Uri(clientPath);
                return uri.LocalPath;
            }
            return null;
        } else {
            return clientPath;
        }
    }

    public static bool HasMonoExtension(this string path) {
        foreach (var ext in MonoExtensions) {
            if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
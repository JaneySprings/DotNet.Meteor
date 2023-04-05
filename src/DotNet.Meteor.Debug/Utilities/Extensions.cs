using System;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using DotNet.Meteor.Debug.Protocol;

namespace DotNet.Meteor.Debug.Utilities;

public static class Extensions {
    public static byte[] ConvertToBytes(this ProtocolMessage obj) {
        var encoding = Encoding.UTF8;
        var asJson = JsonSerializer.Serialize((object)obj);
        byte[] jsonBytes = encoding.GetBytes(asJson);

        string header = string.Format("Content-Length: {0}{1}", jsonBytes.Length, "\r\n\r\n");
        byte[] headerBytes = encoding.GetBytes(header);

        byte[] data = new byte[headerBytes.Length + jsonBytes.Length];
        Buffer.BlockCopy(headerBytes, 0, data, 0, headerBytes.Length);
        Buffer.BlockCopy(jsonBytes, 0, data, headerBytes.Length, jsonBytes.Length);

        return data;
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
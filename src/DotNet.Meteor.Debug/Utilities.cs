using System;
using System.Net;
using System.Net.Sockets;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Debug;

public static class Utilities {
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

    public static IPAddress ResolveIPAddress(string addressString) {
        try {
            if (IPAddress.TryParse(addressString, out IPAddress ipAddress))
                return ipAddress;

            IPHostEntry entry = Dns.GetHostEntry(addressString);

            if (entry?.AddressList?.Length > 0) {
                if (entry.AddressList.Length == 1) {
                    return entry.AddressList[0];
                }
                foreach (IPAddress address in entry.AddressList) {
                    if (address.AddressFamily == AddressFamily.InterNetwork) {
                        return address;
                    }
                }
            }
        } catch (Exception ex) {
            Logger.Log(ex);
        }

        return null;
    }
}
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;

namespace DotNet.Meteor.Debug.Utilities;

public static class Extensions {
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

    public static string ToThreadName(this string threadName, int threadId) {
        if (!string.IsNullOrEmpty(threadName))
            return threadName;
        return $"Thread #{threadId}";
    }

    public static ThreadInfo GetActiveThread(this SoftDebuggerSession session, object sync) {
        lock(sync) {
            return session.ActiveThread;
        }
    }
}
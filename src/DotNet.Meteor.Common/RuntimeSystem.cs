using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace DotNet.Meteor.Common;

public static class RuntimeSystem {
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    public static bool IsAarch64 => RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

    public static string ExecExtension => IsWindows ? ".exe" : "";
    public static string HomeDirectory => IsWindows
        ? Environment.GetEnvironmentVariable("USERPROFILE")!
        : Environment.GetEnvironmentVariable("HOME")!;
    public static string ProgramX86Directory => IsWindows
        ? Environment.GetEnvironmentVariable("ProgramFiles(x86)")!
        : throw new PlatformNotSupportedException();

    public static int GetFreePort() {
        TcpListener? listener = null;
        try {
            listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        } finally {
            listener?.Stop();
        }
    }
}
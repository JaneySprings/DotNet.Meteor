using System;
using System.Runtime.InteropServices;

namespace DotNet.Meteor.Shared {
    public static class RuntimeSystem {
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static string ExecExtension => IsWindows ? ".exe" : "";
        public static string HomeDirectory => IsWindows
            ? Environment.GetEnvironmentVariable("USERPROFILE")
            : Environment.GetEnvironmentVariable("HOME");
        public static string ProgramX86Directory => IsWindows
            ? Environment.GetEnvironmentVariable("ProgramFiles(x86)")
            : null;
    }
}
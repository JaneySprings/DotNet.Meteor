using System.Runtime.InteropServices;

namespace DotNet.Mobile.Shared {
    public static class RuntimeSystem {
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}
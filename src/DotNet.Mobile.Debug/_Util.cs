using System;
using System.IO;

namespace VsCodeMobileUtil {
    public static class Util {
        public static bool IsWindows => Environment.OSVersion.Platform == PlatformID.Win32NT;
    }
}
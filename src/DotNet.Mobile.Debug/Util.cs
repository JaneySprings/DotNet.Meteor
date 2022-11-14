using System;
using System.IO;

namespace VsCodeMobileUtil {
    public static class Util {
        public static bool IsWindows => Environment.OSVersion.Platform == PlatformID.Win32NT;

        //public static void LogToFile(string message)
        //{
        //	var desktop = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "mobile-debug.txt");

        //	File.AppendAllLines(desktop, new[] { message });
        //}
    }
}
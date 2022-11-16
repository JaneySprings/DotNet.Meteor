using System;
using System.IO;

namespace Android.Sdk {
    public static class PathUtils {
        public static string GetSdkLocation() {
            string path = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
            string home = Environment.GetEnvironmentVariable("HOME");

            if (string.IsNullOrEmpty(path))
                path = Path.Combine(home, "Library", "Android", "Sdk");

            if (!Directory.Exists(path))
                throw new Exception("Could not find Android SDK path");

            return path;
        }

        public static FileInfo GetAVDManager() {
            string sdk = GetSdkLocation();
            string path = Path.Combine(sdk, "cmdline-tools", "latest", "bin", "avdmanager");

            if (!File.Exists(path))
                throw new Exception("Could not find avdmanager tool");

            return new FileInfo(path);
        }

        public static FileInfo GetADBTool() {
            string sdk = GetSdkLocation();
            string path = Path.Combine(sdk, "platform-tools", "adb");

            if (!File.Exists(path))
                throw new Exception("Could not find adb tool");

            return new FileInfo(path);
        }

        public static FileInfo GetEmulatorTool() {
            string sdk = GetSdkLocation();
            string path = Path.Combine(sdk, "emulator", "emulator");

            if (!File.Exists(path))
                throw new Exception("Could not find emulator tool");

            return new FileInfo(path);
        }
    }
}
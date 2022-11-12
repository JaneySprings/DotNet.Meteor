using System;
using System.IO;
using DotNet.Mobile.Shared;

namespace Android.Sdk {
    public static class PathUtils {
        public static string GetSdkLocation() {
            string path = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
            string home = Environment.GetEnvironmentVariable("HOME");

            if (string.IsNullOrEmpty(path))
                path = Path.Combine(home, "Library", "Android", "Sdk");

            if (!Directory.Exists(path))
                Logger.Error("Could not find Android SDK path");

            return path;
        }

        public static FileInfo GetAVDManager() {
            string sdk = GetSdkLocation();
            string path = Path.Combine(sdk, "cmdline-tools", "latest", "bin", "avdmanager");

            if (!File.Exists(path))
                Logger.Error("Could not find avdmanager");

            return new FileInfo(path);
        }

        public static FileInfo GetADBTool() {
            string sdk = GetSdkLocation();
            string path = Path.Combine(sdk, "platform-tools", "adb");

            if (!File.Exists(path))
                Logger.Error("Could not find adb");

            return new FileInfo(path);
        }
    }
}
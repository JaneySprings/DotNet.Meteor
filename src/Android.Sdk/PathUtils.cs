using System;
using System.IO;

namespace Android.Sdk {
    public static class PathUtils {
        public static string GetSdkLocation() {
            string path = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
            string home = Environment.GetEnvironmentVariable("HOME");

            if (string.IsNullOrEmpty(path))
                path = Path.Combine(home, "Library", "Android", "sdk");

            if (!Directory.Exists(path))
                throw new Exception("Could not find Android SDK path");

            return path;
        }

        public static string GetAvdLocation() {
            string home = Environment.GetEnvironmentVariable("HOME");
            return Path.Combine(home, ".android", "avd");
        }
    }
}
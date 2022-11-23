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

        public static FileInfo GetAVDManager() {
            string sdk = GetSdkLocation();
            string tools = Path.Combine(sdk, "cmdline-tools");
            FileInfo newestTool = null;

            foreach (string directory in Directory.GetDirectories(tools)) {
                string avdPath = Path.Combine(directory, "bin", "avdmanager");

                if (File.Exists(avdPath)) {
                    var tool = new FileInfo(avdPath);

                    if (newestTool == null || tool.CreationTime > newestTool.CreationTime)
                        newestTool = tool;
                }
            }

            if (newestTool == null || !newestTool.Exists)
                throw new Exception("Could not find avdmanager tool");

            return newestTool;
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
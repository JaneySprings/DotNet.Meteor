using System;
using System.IO;
using DotNet.Mobile.Shared;

namespace Android.Sdk {
    public static class PathUtils {
        public static string SdkLocation() {
            string path = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");

            if (string.IsNullOrEmpty(path)) {
                if (RuntimeSystem.IsWindows)
                    path = InvariantSdk(RuntimeSystem.HomeDirectory, "AppData", "Local", "Android");
                else if (RuntimeSystem.IsMacOS)
                    path = InvariantSdk(RuntimeSystem.HomeDirectory, "Library", "Android");
                else
                    path = InvariantSdk(RuntimeSystem.HomeDirectory, "Android");
            }

            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                throw new Exception("Could not find Android SDK path");

            return path;
        }

        public static string AvdLocation() {
            return Path.Combine(RuntimeSystem.HomeDirectory, ".android", "avd");
        }

        public static FileInfo AdbTool() {
            string sdk = PathUtils.SdkLocation();
            string path = Path.Combine(sdk, "platform-tools", "adb" + RuntimeSystem.ExecExtension);

            if (!File.Exists(path))
                throw new Exception("Could not find adb tool");

            return new FileInfo(path);
        }

        public static FileInfo EmulatorTool() {
            string sdk = PathUtils.SdkLocation();
            string path = Path.Combine(sdk, "emulator", "emulator" + RuntimeSystem.ExecExtension);

            if (!File.Exists(path))
                throw new Exception("Could not find emulator tool");

            return new FileInfo(path);
        }

        public static FileInfo AvdTool() {
            string sdk = PathUtils.SdkLocation();
            string tools = Path.Combine(sdk, "cmdline-tools");
            FileInfo newestTool = null;

            foreach (string directory in Directory.GetDirectories(tools)) {
                string avdPath = Path.Combine(directory, "bin", "avdmanager" + RuntimeSystem.ExecExtension);

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


        private static string InvariantSdk(params string[] tokens) {
            string basePath = Path.Combine(tokens);
            string lowercased = Path.Combine(basePath, "sdk");
            string uppercased = Path.Combine(basePath, "Sdk");

            if (Directory.Exists(lowercased))
                return lowercased;

            if (Directory.Exists(uppercased))
                return uppercased;

            return null;
        }
    }
}
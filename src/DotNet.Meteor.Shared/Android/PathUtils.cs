using System;
using System.IO;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Android {
    public static class PathUtils {
        public static string SdkLocation(Action<string> errorHandler = null) {
            string path = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                return path;

            path = Environment.GetEnvironmentVariable("ANDROID_HOME");
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                return path;

            // Try to find the SDK path in the default AndroidStudio locations
            if (RuntimeSystem.IsWindows)
                path = Path.Combine(RuntimeSystem.HomeDirectory, "AppData", "Local", "Android", "Sdk");
            else if (RuntimeSystem.IsMacOS)
                path = Path.Combine(RuntimeSystem.HomeDirectory, "Library", "Android", "Sdk");
            else
                path = Path.Combine(RuntimeSystem.HomeDirectory, "Android", "Sdk");

            if (Directory.Exists(path))
                return path;

            // Try to find the SDK path in the default VisualStudio locations
            if (RuntimeSystem.IsWindows)
                path = Path.Combine(RuntimeSystem.ProgramX86Directory, "Android", "android-sdk");
            else if (RuntimeSystem.IsMacOS)
                path = Path.Combine(RuntimeSystem.HomeDirectory, "Library", "Developer", "Xamarin", "android-sdk-macosx");

            if (Directory.Exists(path))
                return path;

            errorHandler?.Invoke("Could not find Android SDK path");
            return string.Empty;
        }

        public static string AvdLocation() {
            string path = Environment.GetEnvironmentVariable("ANDROID_USER_HOME");
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                return Path.Combine(path, "avd");

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
    }
}
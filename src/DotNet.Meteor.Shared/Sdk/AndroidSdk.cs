using System;
using System.IO;

namespace DotNet.Meteor.Shared {
    public static class AndroidSdk {
        public static string SdkLocation() {
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

            return string.Empty;
        }

        public static string AvdLocation() {
            string path = Environment.GetEnvironmentVariable("ANDROID_AVD_HOME");
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                return path;

            return Path.Combine(RuntimeSystem.HomeDirectory, ".android", "avd");
        }

        public static FileInfo AdbTool() {
            string sdk = AndroidSdk.SdkLocation();
            string path = Path.Combine(sdk, "platform-tools", "adb" + RuntimeSystem.ExecExtension);

            if (!File.Exists(path))
                throw new FileNotFoundException("Could not find adb tool");

            return new FileInfo(path);
        }

        public static FileInfo EmulatorTool() {
            string sdk = AndroidSdk.SdkLocation();
            string path = Path.Combine(sdk, "emulator", "emulator" + RuntimeSystem.ExecExtension);

            if (!File.Exists(path))
                throw new FileNotFoundException("Could not find emulator tool");

            return new FileInfo(path);
        }

        public static FileInfo AvdTool() {
            string sdk = AndroidSdk.SdkLocation();
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
                throw new FileNotFoundException("Could not find avdmanager tool");

            return newestTool;
        }
    }
}
using System;
using System.IO;
using DotNet.Meteor.Processes;

namespace DotNet.Meteor.Apple {
    public static class PathUtils {
        public static string XCodePath() {
            var selector = new FileInfo(Path.Combine("/usr", "bin", "xcode-select"));
            ProcessResult result = new ProcessRunner(selector, new ProcessArgumentBuilder()
                .Append("-p"))
                .WaitForExit();

            string path = string.Join(Environment.NewLine, result.StandardOutput)?.Trim();

            if (string.IsNullOrEmpty(path))
                throw new Exception("Could not find XCode path");

            return path;
        }

        public static string SimulatorsLocation() {
            string home = Environment.GetEnvironmentVariable("HOME");
            string path = Path.Combine(home, "Library", "Developer", "CoreSimulator", "Devices");

            if (string.IsNullOrEmpty(path))
                throw new Exception("Could not find simulator path");

            return path;
        }

        public static FileInfo SystemProfilerTool() {
            string path = Path.Combine("/usr", "sbin", "system_profiler");
            var tool = new FileInfo(path);

            if (!tool.Exists)
                throw new Exception("Could not find system_profiler path");

            return tool;
        }

        public static FileInfo MLaunchTool() {
            string dotnetPath = Shared.PathUtils.DotNetRootLocation();
            string sdkPath = Path.Combine(dotnetPath, "packs", "Microsoft.iOS.Sdk");
            FileInfo newestTool = null;

            foreach (string directory in Directory.GetDirectories(sdkPath)) {
                string mlaunchPath = Path.Combine(directory, "tools", "bin", "mlaunch");

                if (File.Exists(mlaunchPath)) {
                    var tool = new FileInfo(mlaunchPath);

                    if (newestTool == null || tool.CreationTime > newestTool.CreationTime)
                        newestTool = tool;
                }
            }

            if (newestTool == null || !newestTool.Exists)
                throw new Exception("Could not find mlaunch tool");

            return newestTool;
        }

        public static string IDeviceLocation() {
            string dotnetPath = Shared.PathUtils.DotNetRootLocation();
            string sdkPath = Path.Combine(dotnetPath, "packs", "Microsoft.iOS.Windows.Sdk");
            DirectoryInfo newestTool = null;

            foreach (string directory in Directory.GetDirectories(sdkPath)) {
                string idevicePath = Path.Combine(directory, "tools", "msbuild", "iOS", "imobiledevice-x64");

                if (Directory.Exists(idevicePath)) {
                    var tool = new DirectoryInfo(idevicePath);

                    if (newestTool == null || tool.CreationTime > newestTool.CreationTime)
                        newestTool = tool;
                }
            }

            if (newestTool == null || !newestTool.Exists)
                throw new DirectoryNotFoundException("imobiledevice-x64");

            return newestTool.FullName;
        }

        public static FileInfo XCRunTool() {
            string path = Path.Combine("/usr", "bin", "xcrun");
            FileInfo tool = new FileInfo(path);

            if (!tool.Exists)
                throw new Exception("Could not find xcrun tool");

            return tool;
        }

        public static FileInfo OpenTool() {
            string path = Path.Combine("/usr", "bin", "open");
            FileInfo tool = new FileInfo(path);

            if (!tool.Exists)
                throw new Exception("Could not find open tool");

            return tool;
        }
    }
}
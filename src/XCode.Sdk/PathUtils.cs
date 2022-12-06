using System;
using System.IO;
using DotNet.Mobile.Shared;

namespace XCode.Sdk {
    public static class PathUtils {
        public static string XCodePath() {
            ProcessResult result = ProcessRunner.Execute(
                new FileInfo("/usr/bin/xcode-select"),
                new ProcessArgumentBuilder().Append("-p")
            );

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
            string dotnetPath = Path.Combine("usr", "local", "share", "dotnet");
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

        public static FileInfo XCRunTool() {
            string path = Path.Combine("/usr", "bin", "xcrun");
            FileInfo tool = new FileInfo(path);

            if (!tool.Exists)
                throw new Exception("Could not find xcrun tool");

            return tool;
        }
    }
}
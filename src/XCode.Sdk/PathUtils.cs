using System;
using System.IO;
using DotNet.Mobile.Shared;

namespace XCode.Sdk {
    public static class PathUtils {
        public static string GetXCodePath() {
            ProcessResult result = ProcessRunner.Run(
                new FileInfo("/usr/bin/xcode-select"),
                new ProcessArgumentBuilder().Append("-p")
            );

            string path = string.Join(Environment.NewLine, result.StandardOutput)?.Trim();

            if (string.IsNullOrEmpty(path))
                throw new Exception("Could not find XCode path");

            return path;
        }

        public static FileInfo GetSystemProfiler() {
            string path = Path.Combine("/usr", "sbin", "system_profiler");
            var tool = new FileInfo(path);

            if (!tool.Exists)
                throw new Exception("Could not find system_profiler path");

            return tool;
        }
    }
}
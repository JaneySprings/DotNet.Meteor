using System;
using System.IO;
using System.Linq;

namespace DotNet.Mobile.Shared {
    public static class PathUtils {
        public static string DotNetRootLocation() {
            string dotnet = Environment.GetEnvironmentVariable("DOTNET_SDK_ROOT");

            if (!string.IsNullOrEmpty(dotnet))
                return dotnet;

            string path = Environment.GetEnvironmentVariable("PATH");
            var locations = path.Split(Path.PathSeparator).Where(it => it.Contains("dotnet"));

            if (locations.Any())
                return locations.First();

            throw new FileNotFoundException("Could not find dotnet tool");
        }

        public static FileInfo MSBuildAssembly() {
            var root = DotNetRootLocation();
            var dotnet = DotNetTool();
            var result = new ProcessRunner(dotnet, new ProcessArgumentBuilder()
                .Append("--version"))
                .WaitForExit();
            var dotnetVersion = result.StandardOutput.FirstOrDefault();

            if (string.IsNullOrEmpty(dotnetVersion))
                throw new Exception("Could not find dotnet version");

            var assembly = new FileInfo(Path.Combine(root, "sdk", dotnetVersion, "MSBuild.dll"));

            if (!assembly.Exists)
                throw new FileNotFoundException($"Could not find MSBuild.dll with version ${dotnetVersion}");

            return assembly;
        }

        public static FileInfo DotNetTool() {
            var location = DotNetRootLocation();
            var tool = new FileInfo(Path.Combine(location, "dotnet" + RuntimeSystem.ExecExtension));

            if (tool.Exists) return tool;

            throw new FileNotFoundException("Could not find dotnet executable");
        }

        public static string NormalizePath(this string path) {
            return path
                .Replace('\\', System.IO.Path.DirectorySeparatorChar)
                .Replace('/', System.IO.Path.DirectorySeparatorChar);
        }
    }
}
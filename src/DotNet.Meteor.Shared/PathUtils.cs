using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotNet.Meteor.Processes;

namespace DotNet.Meteor.Shared {
    public static class PathUtils {
        public static string DotNetRootLocation() {
            string dotnet = Environment.GetEnvironmentVariable("DOTNET_ROOT");

            if (!string.IsNullOrEmpty(dotnet) && Directory.Exists(dotnet))
                return dotnet;

            if (RuntimeSystem.IsWindows)
                dotnet = Path.Combine("C:", "Program Files", "dotnet");
            else if (RuntimeSystem.IsMacOS)
                dotnet = Path.Combine("usr", "local", "share", "dotnet");
            else
                dotnet = Path.Combine("usr", "share", "dotnet");

            if (Directory.Exists(dotnet))
                return dotnet;

            var dotnetTool = new FileInfo("dotnet" + RuntimeSystem.ExecExtension);
            var result = new ProcessRunner(dotnetTool, new ProcessArgumentBuilder()
                .Append("--list-sdks"))
                .WaitForExit();

            if (result.ExitCode != 0)
                throw new Exception("Could not find dotnet tool");

            var matches = Regex.Matches(result.StandardOutput.Last(), @"\[(.*?)\]");
            var sdkLocation = matches.FirstOrDefault()?.Groups[1].Value;

            if (string.IsNullOrEmpty(sdkLocation) || !Directory.Exists(sdkLocation))
                throw new Exception("Could not find dotnet sdk");

            return Directory.GetParent(sdkLocation).FullName;
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

        public static string ToPlatformPath(this string path) {
            return path
                .Replace('\\', System.IO.Path.DirectorySeparatorChar)
                .Replace('/', System.IO.Path.DirectorySeparatorChar);
        }
    }
}
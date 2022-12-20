using System;
using System.IO;
using System.Linq;
using DotNet.Mobile.Shared;

namespace DotNet.Mobile.Shared {
    public static class DotNetTool {
        public static void Execute(ProcessArgumentBuilder builder, IProcessLogger logger = null) {
            var dotnet = DotNetLocation();
            var result = new ProcessRunner(dotnet, builder, logger).WaitForExit();

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));
        }

        public static FileInfo DotNetLocation() {
            string dotnet = Environment.GetEnvironmentVariable("DOTNET_SDK_ROOT");

            if (!string.IsNullOrEmpty(dotnet)) {
                var tool = new FileInfo(Path.Combine(dotnet, "dotnet" + RuntimeSystem.ExecExtension));
                if (tool.Exists) return tool;
            }

            string path = Environment.GetEnvironmentVariable("PATH");
            var locations = path.Split(Path.PathSeparator).Where(it => it.Contains("dotnet"));

            foreach(var location in locations) {
                var tool = new FileInfo(Path.Combine(location, "dotnet" + RuntimeSystem.ExecExtension));
                if (tool.Exists) return tool;
            }

            throw new Exception("Could not find dotnet tool");
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotNet.Meteor.Processes;

namespace DotNet.Meteor.Common {
    public static class MicrosoftSdk {
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

            var result = new ProcessRunner("dotnet" + RuntimeSystem.ExecExtension, new ProcessArgumentBuilder()
                .Append("--list-sdks"))
                .WaitForExit();

            if (!result.Success)
                throw new FileNotFoundException("Could not find dotnet tool");

            var matches = Regex.Matches(result.StandardOutput.Last(), @"\[(.*?)\]");
            var sdkLocation = matches.Count != 0 ? matches[0].Groups[1].Value : null;

            if (string.IsNullOrEmpty(sdkLocation) || !Directory.Exists(sdkLocation))
                throw new DirectoryNotFoundException("Could not find dotnet sdk");

            return Directory.GetParent(sdkLocation).FullName;
        }

        public static bool ContainsInsensitive(this string source, string value) {
            return source?.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        public static bool EqualsInsensitive(this string source, string value) {
            return source?.Equals(value, StringComparison.OrdinalIgnoreCase) == true;
        }

        public static string ToPlatformPath(this string path) {
            return path
                .Replace('\\', System.IO.Path.DirectorySeparatorChar)
                .Replace('/', System.IO.Path.DirectorySeparatorChar);
        }
    }
}
using System.Diagnostics;
using System.Text.RegularExpressions;
using DotNet.Meteor.Common.Processes;

namespace DotNet.Meteor.Common.Apple;

public static class AppleSdkLocator {
    public static string XCodePath() {
        var selector = new FileInfo(Path.Combine("/usr", "bin", "xcode-select"));
        var result = new ProcessRunner(selector, new ProcessArgumentBuilder()
            .Append("-p"))
            .WaitForExit();

        var path = string.Join(Environment.NewLine, result.StandardOutput)?.Trim();

        if (string.IsNullOrEmpty(path))
            throw new InvalidOperationException("Could not find XCode path");

        return path;
    }
    public static string SimulatorsLocation() {
        var home = Environment.GetEnvironmentVariable("HOME")!;
        var path = Path.Combine(home, "Library", "Developer", "CoreSimulator", "Devices");

        if (string.IsNullOrEmpty(path))
            throw new InvalidOperationException("Could not find simulator path");

        return path;
    }
    public static string DotNetRootLocation() {
        var dotnet = Environment.GetEnvironmentVariable("DOTNET_ROOT");

        if (!string.IsNullOrEmpty(dotnet) && Directory.Exists(dotnet))
            return dotnet;

        if (RuntimeSystem.IsWindows)
            dotnet = Path.Combine("C:", "Program Files", "dotnet");
        else if (RuntimeSystem.IsMacOS)
            dotnet = Path.Combine("/usr", "local", "share", "dotnet");
        else
            dotnet = Path.Combine("/usr", "share", "dotnet");

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

        return Directory.GetParent(sdkLocation)?.FullName ?? string.Empty;
    }
    public static string IDeviceLocation() {
        var ideviceDirectory = Environment.GetEnvironmentVariable("IDEVICE_DIR");
        if (Directory.Exists(ideviceDirectory))
            return ideviceDirectory;

        if (RuntimeSystem.IsLinux)
            return Path.Combine("/usr", "bin"); // There is no 'Microsoft.iOS.Linux.Sdk' workload

        var sdkPath = string.Empty;
        var dotnetPacksPath = Path.Combine(AppleSdkLocator.DotNetRootLocation(), "packs");
        var sdkPaths = Directory.GetDirectories(dotnetPacksPath, "Microsoft.iOS.Windows.Sdk.net*");
        
        if (sdkPaths.Length > 0)
            sdkPath = sdkPaths.OrderByDescending(x => Path.GetFileName(x)).First();
        if (string.IsNullOrEmpty(sdkPath))
            sdkPath = Path.Combine(dotnetPacksPath, "Microsoft.iOS.Windows.Sdk");
        if (!Directory.Exists(sdkPath))
            throw new DirectoryNotFoundException("Could not find idevice tool");

        var toolLocations = Directory.GetDirectories(sdkPath);
        if (toolLocations.Length == 0)
            throw new FileNotFoundException("Could not find idevice tool");

        var latestToolDirectory = toolLocations.OrderByDescending(x => Path.GetFileName(x)).First();
        return Path.Combine(latestToolDirectory, "tools", "msbuild", "iOS", "imobiledevice-x64");
    }
    public static bool IsAppleDriverRunning() {
        if (RuntimeSystem.IsMacOS)
            throw new PlatformNotSupportedException();

        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("USBMUXD_CHECK_BYPASS")))
            return true;
        
        var processName = RuntimeSystem.IsWindows ? "AppleMobileDeviceProcess" : "usbmuxd";
        var process = Process.GetProcessesByName(processName);
        return process.Length > 0;
    }

    public static FileInfo SystemProfilerTool() {
        string path = Path.Combine("/usr", "sbin", "system_profiler");
        var tool = new FileInfo(path);

        if (!tool.Exists)
            throw new InvalidOperationException("Could not find system_profiler path");

        return tool;
    }
    public static FileInfo MLaunchTool() {
        var mlaunchToolPath = Environment.GetEnvironmentVariable("MLAUNCH_PATH");
        if (File.Exists(mlaunchToolPath))
            return new FileInfo(mlaunchToolPath);

        var sdkPath = string.Empty;
        var dotnetPacksPath = Path.Combine(AppleSdkLocator.DotNetRootLocation(), "packs");
        var sdkPaths = Directory.GetDirectories(dotnetPacksPath, "Microsoft.iOS.Sdk.net*");
        
        if (sdkPaths.Length > 0)
            sdkPath = sdkPaths.OrderByDescending(x => Path.GetFileName(x)).First();
        if (string.IsNullOrEmpty(sdkPath))
            sdkPath = Path.Combine(dotnetPacksPath, "Microsoft.iOS.Sdk");
        if (!Directory.Exists(sdkPath))
            throw new DirectoryNotFoundException("Could not find mlaunch tool");

        var toolLocations = Directory.GetDirectories(sdkPath);
        if (toolLocations.Length == 0)
            throw new FileNotFoundException("Could not find mlaunch tool");

        var latestToolDirectory = toolLocations.OrderByDescending(x => Path.GetFileName(x)).First();
        mlaunchToolPath = Path.Combine(latestToolDirectory, "tools", "bin", "mlaunch");
        return new FileInfo(mlaunchToolPath);
    }
    public static FileInfo XCRunTool() {
        string path = Path.Combine("/usr", "bin", "xcrun");
        FileInfo tool = new FileInfo(path);

        if (!tool.Exists)
            throw new InvalidOperationException("Could not find xcrun tool");

        return tool;
    }
    public static FileInfo OpenTool() {
        string path = Path.Combine("/usr", "bin", "open");
        FileInfo tool = new FileInfo(path);

        if (!tool.Exists)
            throw new InvalidOperationException("Could not find open tool");

        return tool;
    }
}
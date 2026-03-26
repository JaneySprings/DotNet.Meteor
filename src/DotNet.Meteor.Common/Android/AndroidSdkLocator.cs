using DotNet.Meteor.Common.Apple;

namespace DotNet.Meteor.Common.Android;

public static class AndroidSdkLocator {
    public static string SdkLocation() {
        var path = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
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
        var path = Environment.GetEnvironmentVariable("ANDROID_AVD_HOME");
        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            return path;

        return Path.Combine(RuntimeSystem.HomeDirectory, ".android", "avd");
    }

    public static FileInfo AdbTool() {
        string sdk = AndroidSdkLocator.SdkLocation();
        string path = Path.Combine(sdk, "platform-tools", "adb" + RuntimeSystem.ExecExtension);

        if (!File.Exists(path))
            throw new FileNotFoundException("Could not find adb tool");

        return new FileInfo(path);
    }
    public static FileInfo EmulatorTool() {
        string sdk = AndroidSdkLocator.SdkLocation();
        string path = Path.Combine(sdk, "emulator", "emulator" + RuntimeSystem.ExecExtension);

        if (!File.Exists(path))
            throw new FileNotFoundException("Could not find emulator tool");

        return new FileInfo(path);
    }
    public static FileInfo AvdTool() {
        string sdk = AndroidSdkLocator.SdkLocation();
        string tools = Path.Combine(sdk, "cmdline-tools");
        FileInfo? newestTool = null;

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
    public static FileInfo BundleToolJar() {
        var dotnetPacksPath = Path.Combine(AppleSdkLocator.DotNetRootLocation(), "packs");
        foreach (var sdkDir in Directory.GetDirectories(dotnetPacksPath, "Microsoft.Android.Sdk.*").OrderByDescending(x => x)) {
            foreach (var versionDir in Directory.GetDirectories(sdkDir).OrderByDescending(x => x)) {
                var candidate = Path.Combine(versionDir, "tools", "bundletool.jar");
                if (File.Exists(candidate))
                    return new FileInfo(candidate);
            }
        }
        throw new FileNotFoundException("Could not find bundletool.jar in dotnet Android SDK packs");
    }
    public static FileInfo Aapt2Tool() {
        var dotnetPacksPath = Path.Combine(AppleSdkLocator.DotNetRootLocation(), "packs");
        foreach (var sdkDir in Directory.GetDirectories(dotnetPacksPath, "Microsoft.Android.Sdk.*").OrderByDescending(x => x)) {
            foreach (var versionDir in Directory.GetDirectories(sdkDir).OrderByDescending(x => x)) {
                var candidate = Path.Combine(versionDir, "tools", "aapt2" + RuntimeSystem.ExecExtension);
                if (File.Exists(candidate))
                    return new FileInfo(candidate);
            }
        }
        throw new FileNotFoundException("Could not find aapt2 tool in dotnet Android SDK packs");
    }
    public static FileInfo JavaTool() {
        // 1. Check JAVA_HOME
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome)) {
            var path = Path.Combine(javaHome, "bin", "java" + RuntimeSystem.ExecExtension);
            if (File.Exists(path))
                return new FileInfo(path);
        }
        // 2. Check Visual Studio's bundled OpenJDK on Windows (Program Files\Android\openjdk\jdk-*)
        if (RuntimeSystem.IsWindows) {
            var programFiles = Environment.GetEnvironmentVariable("ProgramFiles") ?? @"C:\Program Files";
            var openjdkDir = Path.Combine(programFiles, "Android", "openjdk");
            if (Directory.Exists(openjdkDir)) {
                foreach (var jdkDir in Directory.GetDirectories(openjdkDir, "jdk-*").OrderByDescending(x => x)) {
                    var path = Path.Combine(jdkDir, "bin", "java" + RuntimeSystem.ExecExtension);
                    if (File.Exists(path))
                        return new FileInfo(path);
                }
            }
        }
        throw new FileNotFoundException("Could not find java tool. Set the JAVA_HOME environment variable.");
    }
}
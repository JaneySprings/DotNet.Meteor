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

        if (Directory.Exists(tools)) {
            foreach (string directory in Directory.GetDirectories(tools)) {
                string avdPath = Path.Combine(directory, "bin", "avdmanager" + RuntimeSystem.ExecExtension);

                if (File.Exists(avdPath)) {
                    var tool = new FileInfo(avdPath);

                    if (newestTool == null || tool.CreationTime > newestTool.CreationTime)
                        newestTool = tool;
                }
            }
        }

        if (newestTool == null || !newestTool.Exists)
            throw new FileNotFoundException("Could not find avdmanager tool");

        return newestTool;
    }

    public static FileInfo BundleToolJar() {
        // 1) Try Android SDK locations first.
        string sdk = AndroidSdkLocator.SdkLocation();
        var sdkCandidates = new List<FileInfo>();

        var cmdlineToolsPath = Path.Combine(sdk, "cmdline-tools");
        if (Directory.Exists(cmdlineToolsPath)) {
            foreach (var directory in Directory.GetDirectories(cmdlineToolsPath)) {
                var candidate = Path.Combine(directory, "lib", "bundletool.jar");
                if (File.Exists(candidate))
                    sdkCandidates.Add(new FileInfo(candidate));
            }
        }

        var legacyCandidate = Path.Combine(sdk, "tools", "lib", "bundletool.jar");
        if (File.Exists(legacyCandidate))
            sdkCandidates.Add(new FileInfo(legacyCandidate));

        var newestSdkTool = sdkCandidates.OrderByDescending(x => x.CreationTime).FirstOrDefault();
        if (newestSdkTool != null && newestSdkTool.Exists)
            return newestSdkTool;

        // 2) Fallback to dotnet Android packs (cross-platform).
        var packCandidates = FindAndroidPackTools("bundletool.jar");
        var newestPackTool = packCandidates.OrderByDescending(x => x.CreationTime).FirstOrDefault();
        if (newestPackTool != null && newestPackTool.Exists)
            return newestPackTool;

        throw new FileNotFoundException("Could not find bundletool.jar in Android SDK or dotnet Android packs");
    }

    public static FileInfo Aapt2Tool() {
        // 1) Try Android SDK locations first.
        string sdk = AndroidSdkLocator.SdkLocation();
        var sdkCandidates = new List<FileInfo>();

        var buildToolsPath = Path.Combine(sdk, "build-tools");
        if (Directory.Exists(buildToolsPath)) {
            foreach (var directory in Directory.GetDirectories(buildToolsPath)) {
                var candidate = Path.Combine(directory, "aapt2" + RuntimeSystem.ExecExtension);
                if (File.Exists(candidate))
                    sdkCandidates.Add(new FileInfo(candidate));
            }
        }

        var cmdlineToolsPath = Path.Combine(sdk, "cmdline-tools");
        if (Directory.Exists(cmdlineToolsPath)) {
            foreach (var directory in Directory.GetDirectories(cmdlineToolsPath)) {
                var candidate = Path.Combine(directory, "bin", "aapt2" + RuntimeSystem.ExecExtension);
                if (File.Exists(candidate))
                    sdkCandidates.Add(new FileInfo(candidate));
            }
        }

        var newestSdkTool = sdkCandidates.OrderByDescending(x => x.CreationTime).FirstOrDefault();
        if (newestSdkTool != null && newestSdkTool.Exists)
            return newestSdkTool;

        // 2) Fallback to dotnet Android packs (cross-platform).
        var packCandidates = FindAndroidPackTools("aapt2" + RuntimeSystem.ExecExtension);
        var newestPackTool = packCandidates.OrderByDescending(x => x.CreationTime).FirstOrDefault();
        if (newestPackTool != null && newestPackTool.Exists)
            return newestPackTool;

        throw new FileNotFoundException("Could not find aapt2 tool in Android SDK or dotnet Android packs");
    }

    public static FileInfo JavaTool() {
        // 1. Check JAVA_HOME
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome)) {
            var path = Path.Combine(javaHome, "bin", "java" + RuntimeSystem.ExecExtension);
            if (File.Exists(path))
                return new FileInfo(path);
        }

        // 2. macOS: use /usr/libexec/java_home
        if (RuntimeSystem.IsMacOS) {
            var javaHomeResult = TryRunAndCapture("/usr/libexec/java_home", "");
            if (!string.IsNullOrWhiteSpace(javaHomeResult) && Directory.Exists(javaHomeResult)) {
                var path = Path.Combine(javaHomeResult, "bin", "java");
                if (File.Exists(path))
                    return new FileInfo(path);
            }
        }

        // 3. Check Visual Studio's bundled OpenJDK on Windows (Program Files\Android\openjdk\jdk-*)
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

        // 4. Fallback to PATH lookup
        if (RuntimeSystem.IsWindows) {
            var whereResult = TryRunAndCapture("where", "java");
            var first = whereResult?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(first) && File.Exists(first))
                return new FileInfo(first);
        } else {
            var whichResult = TryRunAndCapture("which", "java");
            if (!string.IsNullOrWhiteSpace(whichResult) && File.Exists(whichResult))
                return new FileInfo(whichResult);
        }

        throw new FileNotFoundException("Could not find java tool. Set JAVA_HOME or ensure java is available on PATH.");
    }

    private static IEnumerable<FileInfo> FindAndroidPackTools(string toolFileName) {
        var candidates = new List<FileInfo>();
        var dotnetRoots = GetDotnetRoots();

        foreach (var dotnetRoot in dotnetRoots) {
            var packsPath = Path.Combine(dotnetRoot, "packs");
            if (!Directory.Exists(packsPath))
                continue;

            foreach (var packName in GetAndroidPackNamesForCurrentPlatform()) {
                var packRoot = Path.Combine(packsPath, packName);
                if (!Directory.Exists(packRoot))
                    continue;

                foreach (var versionDir in Directory.GetDirectories(packRoot).OrderByDescending(x => x)) {
                    var candidate = Path.Combine(versionDir, "tools", toolFileName);
                    if (File.Exists(candidate))
                        candidates.Add(new FileInfo(candidate));
                }
            }

            // Additional broad fallback in case naming conventions evolve.
            foreach (var packRoot in Directory.GetDirectories(packsPath, "Microsoft.Android.Sdk*")) {
                foreach (var versionDir in Directory.GetDirectories(packRoot).OrderByDescending(x => x)) {
                    var candidate = Path.Combine(versionDir, "tools", toolFileName);
                    if (File.Exists(candidate))
                        candidates.Add(new FileInfo(candidate));
                }
            }
        }

        return candidates;
    }

    private static IEnumerable<string> GetAndroidPackNamesForCurrentPlatform() {
        if (RuntimeSystem.IsWindows)
            return new[] { "Microsoft.Android.Sdk.Windows", "Microsoft.Android.Sdk" };
        if (RuntimeSystem.IsMacOS)
            return new[] { "Microsoft.Android.Sdk.Darwin", "Microsoft.Android.Sdk.macOS", "Microsoft.Android.Sdk" };

        return new[] { "Microsoft.Android.Sdk.Linux", "Microsoft.Android.Sdk" };
    }

    private static IEnumerable<string> GetDotnetRoots() {
        var roots = new List<string>();

        var envRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (!string.IsNullOrWhiteSpace(envRoot) && Directory.Exists(envRoot))
            roots.Add(envRoot);

        // Try to infer dotnet root from `dotnet --list-sdks` output:
        // "<version> [<dotnetRoot>/sdk]"
        var sdkList = TryRunAndCapture("dotnet", "--list-sdks");
        if (!string.IsNullOrWhiteSpace(sdkList)) {
            var lines = sdkList.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines) {
                var openBracket = line.IndexOf('[');
                var closeBracket = line.IndexOf(']');
                if (openBracket >= 0 && closeBracket > openBracket) {
                    var sdkPath = line.Substring(openBracket + 1, closeBracket - openBracket - 1).Trim();
                    if (Directory.Exists(sdkPath)) {
                        var sdkDir = new DirectoryInfo(sdkPath);
                        if (sdkDir.Name.Equals("sdk", StringComparison.OrdinalIgnoreCase) && sdkDir.Parent != null) {
                            var root = sdkDir.Parent.FullName;
                            if (Directory.Exists(root))
                                roots.Add(root);
                        }
                    }
                }
            }
        }

        // Common defaults as a final fallback.
        if (RuntimeSystem.IsWindows)
            roots.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet"));
        else if (RuntimeSystem.IsMacOS)
            roots.Add("/usr/local/share/dotnet");
        else
            roots.Add("/usr/share/dotnet");

        return roots
            .Where(Directory.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? TryRunAndCapture(string fileName, string arguments) {
        try {
            var psi = new System.Diagnostics.ProcessStartInfo {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(psi);
            if (process == null)
                return null;

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
                return null;

            return output.Trim();
        }
        catch {
            return null;
        }
    }
}
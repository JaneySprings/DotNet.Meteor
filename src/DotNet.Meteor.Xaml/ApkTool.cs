using System.IO.Compression;

public static class ApkTool {
    public static void ExtractMissingAndroidAssemblies(string assembliesDirectory, Action<string>? logger = null) {
        var apkFile = Directory.GetFiles(assembliesDirectory, "*-Signed.apk", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (apkFile == null) {
            logger?.Invoke($"Could not find *-Signed.apk file in {assembliesDirectory}");
            return;
        }

        var tempDirectory = Path.Combine(assembliesDirectory, "_temp");
        Directory.CreateDirectory(tempDirectory);
        ZipFile.ExtractToDirectory(apkFile, tempDirectory);

        var apkAssembliesDirectory = Path.Combine(tempDirectory, "assemblies");
        foreach (var file in Directory.GetFiles(apkAssembliesDirectory, "*.dll", SearchOption.TopDirectoryOnly)) {
            var fileName = Path.GetFileName(file);
            var targetFile = Path.Combine(assembliesDirectory, fileName);
            File.Copy(file, targetFile, true);
        }

        Directory.Delete(tempDirectory, true);
    }
}
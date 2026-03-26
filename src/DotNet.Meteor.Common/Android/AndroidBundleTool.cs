using DotNet.Meteor.Common.Processes;

namespace DotNet.Meteor.Common.Android;

public static class AndroidBundleTool {
    public static void BuildApks(string aabPath, string apksOutputPath, string serial, KeystoreInfo keystore, string? extraArgs, IProcessLogger? logger = null) {
        var java = AndroidSdkLocator.JavaTool();
        var bundleTool = AndroidSdkLocator.BundleToolJar();
        var adb = AndroidSdkLocator.AdbTool();
        var aapt2 = AndroidSdkLocator.Aapt2Tool();

        var arguments = new ProcessArgumentBuilder()
            .Append("-Xmx1G")
            .Append("-jar").AppendQuoted(bundleTool.FullName)
            .Append("build-apks")
            .Append("--connected-device")
            .Append("--mode", "default")
            .Append("--adb").AppendQuoted(adb.FullName)
            .Append("--device-id", serial)
            .Append("--bundle").AppendQuoted(aabPath)
            .Append("--output").AppendQuoted(apksOutputPath)
            .Append("--aapt2").AppendQuoted(aapt2.FullName)
            .Append("--ks").AppendQuoted(keystore.KeyStorePath)
            .Append("--ks-key-alias", keystore.KeyAlias)
            .Append("--key-pass", $"pass:{keystore.KeyPass}")
            .Append("--ks-pass", $"pass:{keystore.StorePass}");

        if (!string.IsNullOrWhiteSpace(extraArgs)) {
            foreach (var arg in extraArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                arguments.Append(arg);
        }

        var result = new ProcessRunner(java, arguments, logger).WaitForExit();
        if (!result.Success)
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.StandardError));
    }

    public static void InstallApks(string apksPath, string serial, IProcessLogger? logger = null) {
        var java = AndroidSdkLocator.JavaTool();
        var bundleTool = AndroidSdkLocator.BundleToolJar();
        var adb = AndroidSdkLocator.AdbTool();

        var arguments = new ProcessArgumentBuilder()
            .Append("-Xmx1G")
            .Append("-jar").AppendQuoted(bundleTool.FullName)
            .Append("install-apks")
            .Append("--apks").AppendQuoted(apksPath)
            .Append("--adb").AppendQuoted(adb.FullName)
            .Append("--device-id", serial)
            .Append("--allow-downgrade")
            .Append("--modules", "_ALL_");

        var result = new ProcessRunner(java, arguments, logger).WaitForExit();
        if (!result.Success)
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.StandardError));
    }
}
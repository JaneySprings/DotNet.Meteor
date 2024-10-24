using DotNet.Meteor.Common.Processes;

namespace DotNet.Meteor.Common.Android;

public static class AndroidFastDev {
    private static string tempDirectory;

    static AndroidFastDev() {
        var tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "temp");
        tempDirectory = Path.GetFullPath(tempDir);
    }
    
    public static void TryPushAssemblies(DeviceData device, string assembliesPath, string applicationId, IProcessLogger? logger) {
        if (string.IsNullOrEmpty(device.Arch))
            device.Arch = Architectures.X64;

        var appTempDirectory = Path.Combine(tempDirectory, applicationId);
        if (Directory.Exists(appTempDirectory))
            Directory.Delete(appTempDirectory, true);

        logger?.OnOutputDataReceived($"[FastDev]: Copying assemblies to temp directory: {appTempDirectory}");
        CopyDirectory(assembliesPath, Path.Combine(appTempDirectory, device.Arch));

        logger?.OnOutputDataReceived("[FastDev]: Pushing assemblies to device...");
        AndroidDebugBridge.Push(device.Serial, appTempDirectory, $"/data/local/tmp", logger);
        
        logger?.OnOutputDataReceived("[FastDev]: Copying assemblies to app directory");
        AndroidDebugBridge.Shell(device.Serial, "run-as", applicationId, "mkdir", "-p", $"/data/user/0/{applicationId}/files/.__override__/");
        var result = AndroidDebugBridge.ShellResult(device.Serial, "run-as", applicationId, "cp", "-r", $"/data/local/tmp/{applicationId}/{device.Arch}", $"/data/user/0/{applicationId}/files/.__override__/");
        if (!result.Success)
            logger?.OnErrorDataReceived($"[FastDev]: Failed to copy assemblies to app directory: {result.GetError()}");
        
        logger?.OnOutputDataReceived("[FastDev]: Cleaning up temporary directory");
        AndroidDebugBridge.Shell(device.Serial, "rm", "-rf", $"/data/local/tmp/{applicationId}");
    }

    private static void CopyDirectory(string source, string destination) {
        if (!Directory.Exists(destination))
            Directory.CreateDirectory(destination);

        foreach (var file in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories)) {
            var relativePath = Path.GetRelativePath(source, file);
            var destinationPath = Path.Combine(destination, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(file, destinationPath, true);
        }
    }
}
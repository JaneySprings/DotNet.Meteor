using DotNet.Meteor.Common.Extensions;
using DotNet.Meteor.Common.Processes;

namespace DotNet.Meteor.Common.Android;

public static class AndroidFastDev {
    public static void TryPushAssemblies(DeviceData device, string assetsPath, string applicationId, IProcessLogger? logger) {
        if (string.IsNullOrEmpty(assetsPath) || !Directory.Exists(assetsPath)) {
            logger?.OnErrorDataReceived($"[FastDev]: Path '{assetsPath}' is not valid or does not exist.");
            return;
        }

        assetsPath = assetsPath.TrimPathEnd();

        logger?.OnOutputDataReceived($"[FastDev]: Pushing '{assetsPath}' to device...");
        AndroidDebugBridge.Shell(device.Serial, "mkdir", "-p", $"/data/local/tmp/{applicationId}");
        AndroidDebugBridge.Push(device.Serial, assetsPath, $"/data/local/tmp/{applicationId}", logger);
        
        logger?.OnOutputDataReceived("[FastDev]: Deleting existing assemblies in app directory");
        AndroidDebugBridge.Shell(device.Serial, "run-as", applicationId, "mkdir", "-p", $"/data/user/0/{applicationId}/files"); // Create directory if not exists
        AndroidDebugBridge.Shell(device.Serial, "run-as", applicationId, "rm", "-rf", $"/data/user/0/{applicationId}/files/.__override__"); // Ensure directory is empty
        
        logger?.OnOutputDataReceived("[FastDev]: Copying assemblies to app directory");
        var assetsName = Path.GetFileName(assetsPath);
        var result = AndroidDebugBridge.ShellResult(device.Serial, "run-as", applicationId, "cp", "-r", $"/data/local/tmp/{applicationId}/{assetsName}", $"/data/user/0/{applicationId}/files/.__override__");
        if (!result.Success)
            logger?.OnErrorDataReceived($"[FastDev]: Failed to copy assemblies to app directory: {result.GetError()}");
        
        logger?.OnOutputDataReceived("[FastDev]: Cleaning up temporary directory");
        AndroidDebugBridge.Shell(device.Serial, "rm", "-rf", $"/data/local/tmp/{applicationId}");
    }
}
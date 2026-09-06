export const extensionId = "dotnet-meteor";
export const dotrushExtensionId = "nromanov.dotrush";

export const commandIdSelectActiveDevice = "dotnet-meteor.selectActiveDevice";
export const commandTitleSelectActiveDevice = "select device";
export const commandIdActiveDeviceName = "dotnet-meteor.activeDeviceName";
export const commandIdActiveDeviceSerial = "dotnet-meteor.activeDeviceSerial";
export const commandIdTriggerHotReload = "dotnet-meteor.triggerHotReload";
export const commandIdXamlReplaceCode = "dotnet-meteor.xaml.replaceCode";

export const messageInvalidDotnetSdk = "Failed to run the .NET SDK 10. Please make sure the .NET SDK 10 or newer is installed.";
export const messageNoFrameworkFound = "Supported framework not found. Make sure you have selected a correct device.";
export const messageClrIosNotSupported = "CoreCLR is not supported on iOS in .NET 10.0";
export const messageClrMacNotSupported = "CoreCLR is not supported on Maccatalyst in .NET 10.0";
export const messageNoProjectFound = "Selected project not found. Make sure you have selected a project in the status bar.";
export const messageNoDeviceFound = "Incorrect selected device. Make sure you have selected a device.";
export const messageDeviceLoading = "Fetching devices...";
export const messageMissingCoreclrHost = "The 'dotnetMeteor.debuggerOptions.remoteCoreclrHost' setting is not configured. Please set it in VS Code settings and try again.";
export const messageMissingCoreclrTarget = "The 'dotnetMeteor.debuggerOptions.remoteCoreclrTarget' setting is not configured. Please set it in VS Code settings and try again.";

export const taskDefinitionId = "dotnet-meteor.task";
export const debuggerMeteorId = "dotnet-meteor.debugger";
export const configId = "dotnetMeteor";
export const configDotRushId = "dotrush";

export const configIdHotReloadHostPort = "hotReloadHostPort";
export const configIdUninstallApplication = "uninstallApplicationBeforeInstalling";
export const configIdApplyHotReloadChangesOnSave = "applyHotReloadChangesOnSave";
export const configIdAndroidPort = "debuggerOptions.androidPort";
export const configIdApplePort = "debuggerOptions.applePort";
export const configIdRemoteCoreclrTarget = "debuggerOptions.remoteCoreclrTarget";
export const configIdRemoteCoreclrHost = "debuggerOptions.remoteCoreclrHost";

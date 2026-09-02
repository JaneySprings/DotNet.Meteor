export const extensionId = "dotnet-meteor";
export const extensionPublisher = "nromanov";
export const extensionDisplayName = ".NET Meteor";

export const dotrushExtensionId = "nromanov.dotrush";

export const commandIdSelectActiveDevice = "dotnet-meteor.selectActiveDevice";
export const commandTitleSelectActiveDevice = "select device";
export const commandIdActiveDeviceName = "dotnet-meteor.activeDeviceName";
export const commandIdActiveDeviceSerial = "dotnet-meteor.activeDeviceSerial";
export const commandIdTriggerHotReload = "dotnet-meteor.triggerHotReload";
export const commandIdXamlReplaceCode = "dotnet-meteor.xaml.replaceCode";
export const commandIdPairToMac = "dotnet-meteor.pairToMac";

export const taskDefinitionId = "dotnet-meteor.task";
export const taskDefinitionDefaultTarget = "build";
export const taskDefinitionDefaultTargetCapitalized = "Build";
export const taskProblemMatcherId = "dotnet-meteor.problemMatcher";

export const debuggerMeteorId = "dotnet-meteor.debugger";
export const debuggerMeteorTitle = ".NET Meteor Debugger";

export const messageInvalidDotnetSdk = "Failed to run the .NET SDK 10. Please make sure the .NET SDK 10 or newer is installed.";
export const messageDeviceNotExists = "Selected device does not exist anymore. Make sure that the chosen device is connected to your machine.";
export const messageNoFrameworkFound = "Supported framework not found. Make sure you have selected a correct device.";
export const messageClrIosNotSupported = "CoreCLR is not supported on iOS in .NET 10.0";
export const messageClrMacNotSupported = "CoreCLR is not supported on Maccatalyst in .NET 10.0";
export const messageNoProjectFound = "Selected project not found. Make sure you have selected a project in the status bar.";
export const messageNoDeviceFound = "Incorrect selected device. Make sure you have selected a device.";
export const messageDeviceLoading = "Fetching devices...";
export const messageMissingCoreclrHost = "The 'dotnetMeteor.debuggerOptions.remoteCoreclrHost' setting is not configured. Please set it in VS Code settings and try again.";
export const messageMissingCoreclrTarget = "The 'dotnetMeteor.debuggerOptions.remoteCoreclrTarget' setting is not configured. Please set it in VS Code settings and try again.";

export const configId = "dotnetMeteor";
export const configDotRushId = "dotrush";

export const configIdHotReloadHostPort = "hotReloadHostPort";
export const configIdUninstallApplication = "uninstallApplicationBeforeInstalling";
export const configIdApplyHotReloadChangesOnSave = "applyHotReloadChangesOnSave";
export const configIdAndroidPort = "debuggerOptions.androidPort";
export const configIdApplePort = "debuggerOptions.applePort";
export const configIdRemoteCoreclrTarget = "debuggerOptions.remoteCoreclrTarget";
export const configIdRemoteCoreclrHost = "debuggerOptions.remoteCoreclrHost";

export const configIdProjectAssembliesOnly = "debugger.projectAssembliesOnly";
export const configIdStepOverPropertiesAndOperators = "debugger.stepOverPropertiesAndOperators";
export const configIdSymbolSearchPaths = "debugger.symbolSearchPaths";
export const configIdSearchMicrosoftSymbolServer = "debugger.searchMicrosoftSymbolServer";
export const configIdAutomaticSourcelinkDownload = "debugger.automaticSourcelinkDownload";
// export const configIdSourceCodeMappings = "debuggerOptions.sourceCodeMappings";


export const extensionId = "dotnet-meteor";
export const extensionPublisher = "nromanov";

export const commandIdSelectActiveProject = "dotnet-meteor.selectActiveProject";
export const commandTitleSelectActiveProject = ".NET Meteor: Select workspace project";

export const commandIdSelectActiveConfiguration = "dotnet-meteor.selectActiveConfiguration";
export const commandTitleSelectActiveConfiguration = ".NET Meteor: Select project configuration";

export const commandIdSelectActiveDevice = "dotnet-meteor.selectActiveDevice";
export const commandTitleSelectActiveDevice = ".NET Meteor: Select device";

export const commandIdFocusOnDebug = "workbench.debug.action.focusRepl";

export const taskDefinitionId = "dotnet-meteor.task";
export const taskDefinitionDefaultTarget = "Build";
export const taskProblemMatcherId = "dotnet-meteor.problemMatcher";

export const debuggerMeteorId = "dotnet-meteor.debugger";
export const debuggerMeteorTitle = ".NET Meteor Debugger";

export const messageDebugNotSupported = "Unable to debug an application when it is run in release configuration. Run the application in release configuration without debugger.";
export const messageDebugNotSupportedWin = "Mono Debugger cannot be attached to WinUI applications. You can run the application without debugger.";
export const messageDeviceNotExists = "Selected device does not exist anymore. Make sure that the choosen device is connected to your machine.";
export const messageNoFrameworkFound = "Supported framework not found. Make sure you have selected a correct device.";
export const messageNoProjectFound = "Selected project not found. Make sure you have selected a project in the status bar.";
export const messageNoDeviceFound = "Incorrect selected device. Make sure you have selected a device.";
export const messageDeviceLoading = "Fetching devices...";
export const messageRuntimeNotFound= ".NET Meteor requires LTS version of .NET SDK (.NET 6.0 LTS and higher).";

export const configId = "dotnetMeteor";

export const configIdMonoSdbDebuggerPortAndroid = "monoSdbDebuggerPortAndroid";
export const configDefaultMonoSdbDebuggerPortAndroid = 10000;

export const configIdMonoSdbDebuggerPortApple = "monoSdbDebuggerPortApple";
export const configDefaultMonoSdbDebuggerPortApple = 55551;

export const configIdHotReloadHostPort = "hotReloadHostPort";
export const configDefaultHotReloadHostPort = 9988;

export const configIdUninstallApplicationBeforeInstalling = "uninstallApplicationBeforeInstalling";
export const configDefaultUninstallApplicationBeforeInstalling = true;

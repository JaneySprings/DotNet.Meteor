
export const extensionId = "dotnet-meteor";
export const extensionPublisher = "nromanov";
export const extensionDisplayName = ".NET Meteor";

export const commandIdSelectActiveProject = "dotnet-meteor.selectActiveProject";
export const commandTitleSelectActiveProject = "Select workspace project";

export const commandIdSelectActiveConfiguration = "dotnet-meteor.selectActiveConfiguration";
export const commandTitleSelectActiveConfiguration = "Select project configuration";

export const commandIdSelectActiveDevice = "dotnet-meteor.selectActiveDevice";
export const commandTitleSelectActiveDevice = "Select device";

export const commandIdActiveProjectPath = "dotnet-meteor.activeProjectPath";
export const commandIdActiveConfiguration = "dotnet-meteor.activeConfiguration";
export const commandIdActiveTargetFramework = "dotnet-meteor.activeTargetFramework";
export const commandIdActiveDeviceName = "dotnet-meteor.activeDeviceName";
export const commandIdActiveDeviceSerial = "dotnet-meteor.activeDeviceSerial";
export const commandIdTriggerHotReload = "dotnet-meteor.triggerHotReload";
export const commandIdXamlReplaceCode = "dotnet-meteor.xaml.replaceCode";

export const taskDefinitionId = "dotnet-meteor.task";
export const taskDefinitionDefaultTarget = "build";
export const taskDefinitionDefaultTargetCapitalized = "Build";
export const taskProblemMatcherId = "dotnet-meteor.problemMatcher";

export const debuggerMeteorId = "dotnet-meteor.debugger";
export const debuggerMeteorTitle = ".NET Meteor Debugger";

export const extendedViewIdModules = "dotnet-meteor.modulesView";

export const messageDebugNotSupported = "Unable to debug an application when profiler is enabled. Run the application without debugger.";
export const messageDebugNotSupportedWin = "Mono Debugger cannot be attached to WinUI applications. You can run the application without debugger.";
export const messageDeviceNotExists = "Selected device does not exist anymore. Make sure that the chosen device is connected to your machine.";
export const messageNoFrameworkFound = "Supported framework not found. Make sure you have selected a correct device.";
export const messageNoProjectFound = "Selected project not found. Make sure you have selected a project in the status bar.";
export const messageNoDeviceFound = "Incorrect selected device. Make sure you have selected a device.";
export const messageDeviceLoading = "Fetching devices...";

export const configId = "dotnetMeteor";

export const configIdMonoSdbDebuggerPortAndroid = "monoSdbDebuggerPortAndroid";
export const configDefaultMonoSdbDebuggerPortAndroid = 10000;

export const configIdMonoSdbDebuggerPortApple = "monoSdbDebuggerPortApple";
export const configDefaultMonoSdbDebuggerPortApple = 55551;

export const configIdHotReloadHostPort = "hotReloadHostPort";
export const configDefaultHotReloadHostPort = 9988;

export const configIdProfilerHostPort = "profilerHostPort";
export const configDefaultProfilerHostPort = 9000;

export const configIdUninstallApplicationBeforeInstalling = "uninstallApplicationBeforeInstalling";
export const configIdApplyHotReloadChangesOnSave = "applyHotReloadChangesOnSave";

export const configIdDebuggerOptions = "debuggerOptions";
export const configIdDebuggerOptionsEvaluationTimeout = `${configIdDebuggerOptions}.evaluationTimeout`;
export const configIdDebuggerOptionsMemberEvaluationTimeout = `${configIdDebuggerOptions}.memberEvaluationTimeout`;
export const configIdDebuggerOptionsAllowTargetInvoke = `${configIdDebuggerOptions}.allowTargetInvoke`;
export const configIdDebuggerOptionsAllowMethodEvaluation = `${configIdDebuggerOptions}.allowMethodEvaluation`;
export const configIdDebuggerOptionsAllowToStringCalls = `${configIdDebuggerOptions}.allowToStringCalls`;
export const configIdDebuggerOptionsFlattenHierarchy = `${configIdDebuggerOptions}.flattenHierarchy`;
export const configIdDebuggerOptionsGroupPrivateMembers = `${configIdDebuggerOptions}.groupPrivateMembers`;
export const configIdDebuggerOptionsGroupStaticMembers = `${configIdDebuggerOptions}.groupStaticMembers`;
export const configIdDebuggerOptionsUseExternalTypeResolver = `${configIdDebuggerOptions}.useExternalTypeResolver`;
export const configIdDebuggerOptionsIntegerDisplayFormat = `${configIdDebuggerOptions}.integerDisplayFormat`;
export const configIdDebuggerOptionsCurrentExceptionTag = `${configIdDebuggerOptions}.currentExceptionTag`;
export const configIdDebuggerOptionsEllipsizeStrings = `${configIdDebuggerOptions}.ellipsizeStrings`;
export const configIdDebuggerOptionsEllipsizedLength = `${configIdDebuggerOptions}.ellipsizedLength`;
export const configIdDebuggerOptionsProjectAssembliesOnly = `${configIdDebuggerOptions}.projectAssembliesOnly`;
export const configIdDebuggerOptionsStepOverPropertiesAndOperators = `${configIdDebuggerOptions}.stepOverPropertiesAndOperators`;
export const configIdDebuggerOptionsSearchMicrosoftSymbolServer = `${configIdDebuggerOptions}.searchMicrosoftSymbolServer`;
export const configIdDebuggerOptionsSearchNuGetSymbolServer = `${configIdDebuggerOptions}.searchNugetSymbolServer`;
export const configIdDebuggerOptionsSourceCodeMappings = `${configIdDebuggerOptions}.sourceCodeMappings`;
export const configIdDebuggerOptionsAutomaticSourcelinkDownload = `${configIdDebuggerOptions}.automaticSourcelinkDownload`;
export const configIdDebuggerOptionsSymbolSearchPaths = `${configIdDebuggerOptions}.symbolSearchPaths`;
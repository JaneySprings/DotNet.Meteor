import { window, workspace, ExtensionContext, DebugConfiguration } from 'vscode';
import { Interop } from '../interop/interop';
import { StatusBarController } from "./statusbarController";
import { Project } from '../models/project';
import { Device } from '../models/device';
import * as res from '../resources/constants';
import * as path from 'path';

export class ConfigurationController {
    public static androidSdkDirectory: string | undefined;
    public static noDebug: boolean | undefined;
    public static profiler: string | undefined;
    public static project: Project | undefined;
    public static device: Device | undefined;
    public static configuration: string | undefined;

    public static onWindows: boolean = process.platform === 'win32';
    public static onLinux: boolean = process.platform === 'linux';
    public static onMac: boolean = process.platform === 'darwin';

    public static activate(context: ExtensionContext) {
        ConfigurationController.androidSdkDirectory = Interop.getAndroidSdk();
    }

    public static isMacCatalyst() { return ConfigurationController.device?.platform === 'maccatalyst'; }
    public static isWindows() { return ConfigurationController.device?.platform === 'windows'; }
    public static isAndroid() { return ConfigurationController.device?.platform === 'android'; }
    public static isAppleMobile() { return ConfigurationController.device?.platform === 'ios'; }

    public static isValid(): boolean {
        if (!ConfigurationController.project?.path) {
            window.showErrorMessage(res.messageNoProjectFound, { modal: true });
            return false;
        }
        if (!ConfigurationController.device?.platform) {
            window.showErrorMessage(res.messageNoDeviceFound, { modal: true });
            return false;
        }
        if (!ConfigurationController.getTargetFramework()) {
            window.showErrorMessage(res.messageNoFrameworkFound, { modal: true });
            return false;
        }
        if (!ConfigurationController.noDebug && ConfigurationController.profiler) {
			window.showErrorMessage(res.messageDebugWithProfilerNotSupported, { modal: true });
			return false;
		}
        if (!StatusBarController.devices.some(it => it.name === ConfigurationController.device?.name)) {
            window.showErrorMessage(res.messageDeviceNotExists, { modal: true });
            return false;
        }

        return true;
    }
    public static isActive(): boolean {
        return ConfigurationController.project !== undefined && ConfigurationController.device !== undefined;
    }

    public static getDebuggingPort(): number {
        if (ConfigurationController.isAndroid())
            return ConfigurationController.getSetting(res.configIdMonoSdbDebuggerPortAndroid, res.configDefaultMonoSdbDebuggerPortAndroid);

        if (ConfigurationController.isAppleMobile() && !ConfigurationController.device?.is_emulator)
            return ConfigurationController.onMac
                ? ConfigurationController.getSetting(res.configIdMonoSdbDebuggerPortApple, res.configDefaultMonoSdbDebuggerPortApple) 
                : 10000; /* We can't specify the port on Windows or Linux, so we use the default one */

        return 0;
    }
    public static getReloadHostPort(): number {
        return ConfigurationController.getSetting<number>(res.configIdHotReloadHostPort, res.configDefaultHotReloadHostPort);
    }
    public static getProfilerPort(): number {
        return ConfigurationController.getSetting<number>(res.configIdProfilerHostPort, res.configDefaultProfilerHostPort);
    }
    public static getUninstallAppOption(): boolean {
        return ConfigurationController.getSetting<boolean>(res.configIdUninstallApplicationBeforeInstalling, true);
    }
    public static getTargetFramework(): string | undefined {
        const framework = ConfigurationController.project?.frameworks.find(it => it.includes(ConfigurationController.device?.platform ?? 'undefined'));
        if (framework === undefined && (ConfigurationController.isWindows() || ConfigurationController.isMacCatalyst()))
            return ConfigurationController.project?.frameworks.find(it => !it.includes('-'));

        return framework;
    }
    public static getDebuggerOptions(): any {
        return {
            evaluationOptions: {
                evaluationTimeout: ConfigurationController.getSettingOrDefault<number>(res.configIdDebuggerOptionsEvaluationTimeout),
                memberEvaluationTimeout: ConfigurationController.getSettingOrDefault<number>(res.configIdDebuggerOptionsMemberEvaluationTimeout),
                allowTargetInvoke: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsAllowTargetInvoke),
                allowMethodEvaluation: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsAllowMethodEvaluation),
                allowToStringCalls: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsAllowToStringCalls),
                flattenHierarchy: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsFlattenHierarchy),
                groupPrivateMembers: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsGroupPrivateMembers),
                groupStaticMembers: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsGroupStaticMembers),
                useExternalTypeResolver: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsUseExternalTypeResolver),
                integerDisplayFormat: ConfigurationController.getSettingOrDefault<string>(res.configIdDebuggerOptionsIntegerDisplayFormat),
                currentExceptionTag: ConfigurationController.getSettingOrDefault<string>(res.configIdDebuggerOptionsCurrentExceptionTag),
                ellipsizeStrings: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsEllipsizeStrings),
                ellipsizedLength: ConfigurationController.getSettingOrDefault<number>(res.configIdDebuggerOptionsEllipsizedLength),
                stackFrameFormat: {
                    line: false, // VSCode already shows the line number
                    module: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsStackFrameFormatModule),
                    parameterTypes: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsStackFrameFormatParameterTypes),
                    parameterValues: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsStackFrameFormatParameterValues),
                    parameterNames: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsStackFrameFormatParameterNames),
                    language: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsStackFrameFormatLanguage),
                },
            },
            stepOverPropertiesAndOperators: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsStepOverPropertiesAndOperators),
            projectAssembliesOnly: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsProjectAssembliesOnly),
            automaticSourceLinkDownload: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsAutomaticSourcelinkDownload),
            symbolSearchPaths: ConfigurationController.getSettingOrDefault<string[]>(res.configIdDebuggerOptionsSymbolSearchPaths),
            sourceCodeMappings: ConfigurationController.getSettingOrDefault<any>(res.configIdDebuggerOptionsSourceCodeMappings),
            searchMicrosoftSymbolServer: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsSearchMicrosoftSymbolServer),
            skipNativeTransitions: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsSkipNativeTransitions),
        };
    }
    public static convertMonoToVsdbgOptions(config: DebugConfiguration): DebugConfiguration {
        config.justMyCode = false; //ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsProjectAssembliesOnly);
        config.enableStepFiltering = ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsStepOverPropertiesAndOperators);
        config.symbolOptions = {
            searchPaths: ConfigurationController.getSettingOrDefault<string[]>(res.configIdDebuggerOptionsSymbolSearchPaths),
            searchMicrosoftSymbolServer: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsSearchMicrosoftSymbolServer),
        };
        config.sourceLinkOptions = {
            "*": { enabled: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsAutomaticSourcelinkDownload) }
        }
        return config;
    }
    public static getSetting<TResult>(id: string, fallback: TResult): TResult {
        return workspace.getConfiguration(res.configId).get(id) ?? fallback;
    }
    public static getSettingOrDefault<TResult>(id: string): TResult | undefined {
        return workspace.getConfiguration(res.configId).get(id);
    }
    public static getProgramPath(project: Project, configuration: string, device: Device): string | undefined {
        const targetPath = Interop.getPropertyValue('TargetPath', project, configuration, device);
        if (targetPath === undefined)
            return undefined;

        if (ConfigurationController.isAndroid()) {
            const outDir = path.dirname(targetPath);
            const packageName = Interop.getPropertyValue('ApplicationId', project, configuration, device);
            if (packageName !== undefined)
                return path.join(outDir, packageName + '-Signed.apk');
        }
        if (ConfigurationController.isWindows()) {
            const targetDirectory = path.dirname(targetPath);
            const targetFile = path.basename(targetPath, '.dll');
            return path.join(targetDirectory, targetFile + '.exe');
        }
        if (ConfigurationController.isAppleMobile() || ConfigurationController.isMacCatalyst()) {
            const outDir = path.dirname(targetPath);
            const bundleName = Interop.getPropertyValue('_AppBundleName', project, configuration, device);
            const bundleExt = ConfigurationController.onMac ? '.app' : '.ipa';
            if (bundleName !== undefined)
                return path.join(outDir, bundleName + bundleExt);
        }

        return targetPath;
    }
    public static getAssetsPath(project: Project, configuration: string, device: Device): string | undefined {
        if (!ConfigurationController.isAndroid())
            return undefined;

        const assembliesDir = Interop.getPropertyValue('MonoAndroidIntermediateAssemblyDir', project, configuration, device);
        return assembliesDir;
    }

    public static isVsdbgRequired(): boolean {
        const framework = ConfigurationController.getTargetFramework();
        if (framework === undefined)
            return true;

        return !framework.includes('-android') && !framework.includes('-ios') && !framework.includes('-maccatalyst');
    }
} 
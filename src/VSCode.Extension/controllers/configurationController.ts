import { window, workspace, ExtensionContext } from 'vscode';
import { InteropController } from './interopController';
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

    public static activate(context: ExtensionContext) {
        ConfigurationController.androidSdkDirectory = InteropController.getAndroidSdk();
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
            return ConfigurationController.getSetting(res.configIdMonoSdbDebuggerPortApple, res.configDefaultMonoSdbDebuggerPortApple);

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
        return ConfigurationController.project?.frameworks.find(it => {
            return it.includes(ConfigurationController.device?.platform ?? 'undefined');
        });
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
                StackFrameFormat: {

                },
            },
            stepOverPropertiesAndOperators: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsStepOverPropertiesAndOperators),
            projectAssembliesOnly: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsProjectAssembliesOnly),
            automaticSourceLinkDownload: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsAutomaticSourcelinkDownload),
            symbolSearchPaths: ConfigurationController.getSettingOrDefault<string[]>(res.configIdDebuggerOptionsSymbolSearchPaths),
            sourceCodeMappings: ConfigurationController.getSettingOrDefault<any>(res.configIdDebuggerOptionsSourceCodeMappings),
            searchMicrosoftSymbolServer: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsSearchMicrosoftSymbolServer),
        };
    }
    public static getSetting<TResult>(id: string, fallback: TResult): TResult {
        return workspace.getConfiguration(res.configId).get(id) ?? fallback;
    }
    public static getSettingOrDefault<TResult>(id: string): TResult | undefined {
        return workspace.getConfiguration(res.configId).get(id);
    }
    public static getProgramPath(project: Project, configuration: string, device: Device): string | undefined {
        const targetPath = InteropController.getPropertyValue('TargetPath', project, configuration, device);
        if (targetPath === undefined)
            return undefined;

        if (ConfigurationController.isAndroid()) {
            const outDir = path.dirname(targetPath);
            const packageName = InteropController.getPropertyValue('ApplicationId', project, configuration, device);
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
            const bundleName = InteropController.getPropertyValue('_AppBundleName', project, configuration, device);
            if (bundleName !== undefined)
                return path.join(outDir, bundleName + '.app');
        }

        return undefined;
    }
    public static getAssetsPath(project: Project, configuration: string, device: Device): string | undefined {
        if (!ConfigurationController.isAndroid())
            return undefined;

        const assembliesDir = InteropController.getPropertyValue('MonoAndroidIntermediateAssemblyDir', project, configuration, device);
        return assembliesDir;
    }
} 
import { StatusBarController } from "./statusbarController";
import { Interop } from '../interop/interop';
import { Project } from '../models/project';
import { Device } from '../models/device';
import * as res from '../resources/constants';
import * as vscode from 'vscode';
import * as path from 'path';

export class ConfigurationController {
    public static androidSdkDirectory: string | undefined;
    public static project: Project | undefined;
    public static device: Device | undefined;
    public static configuration: string | undefined;
    public static targetFramework: string | undefined;

    public static onWindows: boolean = process.platform === 'win32';
    public static onMac: boolean = process.platform === 'darwin';

    public static activate(context: vscode.ExtensionContext) {
        ConfigurationController.androidSdkDirectory = Interop.getAndroidSdk();
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveDeviceName, () => ConfigurationController.device?.name));
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveDeviceSerial, () => ConfigurationController.device?.serial));
    }

    public static isMacCatalyst() { return ConfigurationController.device?.platform === 'maccatalyst'; }
    public static isWindows() { return ConfigurationController.device?.platform === 'windows'; }
    public static isAndroid() { return ConfigurationController.device?.platform === 'android'; }
    public static isAppleMobile() { return ConfigurationController.device?.platform === 'ios'; }

    public static isValid(): boolean {
        if (!ConfigurationController.project?.path) {
            vscode.window.showErrorMessage(res.messageNoProjectFound, { modal: true });
            return false;
        }
        if (!ConfigurationController.device?.platform) {
            vscode.window.showErrorMessage(res.messageNoDeviceFound, { modal: true });
            return false;
        }
        if (!ConfigurationController.targetFramework) {
            vscode.window.showErrorMessage(res.messageNoFrameworkFound, { modal: true });
            return false;
        }
        if (!StatusBarController.devices.some(it => it.name === ConfigurationController.device?.name)) {
            vscode.window.showErrorMessage(res.messageDeviceNotExists, { modal: true });
            return false;
        }

        return true;
    }
    public static isActive(): boolean {
        return ConfigurationController.project !== undefined && ConfigurationController.device !== undefined;
    }

    public static getDebuggingPort(): number {
        if (ConfigurationController.isAndroid())
            return ConfigurationController.getSetting(res.configIdAndroidPort, 10000);

        if (ConfigurationController.isAppleMobile() && !ConfigurationController.device?.is_emulator)
            return ConfigurationController.onMac
                ? ConfigurationController.getSetting(res.configIdApplePort, 55551)
                : 10000; /* We can't specify the port on Windows or Linux, so we use the default one */

        return 0;
    }
    public static getReloadHostPort(): number {
        return ConfigurationController.getSetting<number>(res.configIdHotReloadHostPort, 9988);
    }
    public static getUninstallAppOption(): boolean {
        return ConfigurationController.getSetting<boolean>(res.configIdUninstallApplicationBeforeInstalling, true);
    }
    // public static getTargetFramework(): string | undefined {
    //     const framework = ConfigurationController.project?.frameworks.find(it => it.includes(ConfigurationController.device?.platform ?? 'undefined'));
    //     if (framework === undefined && (ConfigurationController.isWindows() || ConfigurationController.isMacCatalyst()))
    //         return ConfigurationController.project?.frameworks.find(it => !it.includes('-'));

    //     return framework;
    // }

    public static getSetting<TResult>(id: string, fallback: TResult): TResult {
        return vscode.workspace.getConfiguration(res.configId).get(id) ?? fallback;
    }
    public static getSettingOrDefault<TResult>(id: string): TResult | undefined {
        return vscode.workspace.getConfiguration(res.configId).get(id);
    }

    public static getProgramPath(project: Project, configuration: string, device: Device): string | undefined {
        const targetPath = Interop.getPropertyValue('TargetPath', project, configuration, device);
        if (targetPath === undefined)
            return undefined;

        if (ConfigurationController.isWindows()) {
            const targetDirectory = path.dirname(targetPath);
            const targetFile = path.basename(targetPath, '.dll');
            return path.join(targetDirectory, targetFile + '.exe');
        }
        if (ConfigurationController.isAndroid()) {
            const outDir = path.dirname(targetPath);
            const packageName = Interop.getPropertyValue('ApplicationId', project, configuration, device);
            if (packageName !== undefined)
                return path.join(outDir, packageName + '-Signed.apk');
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
    public static getAssetsPath(program: string, project: Project, configuration: string, device: Device): string | undefined {
        if (ConfigurationController.isAndroid()) {
            const assembliesDir = Interop.getPropertyValue('MonoAndroidIntermediateAssemblyDir', project, configuration, device);
            return assembliesDir;
        }
        if (ConfigurationController.isMacCatalyst()) {
            const assembliesDir = path.join(program, "Contents", "MonoBundle");
            return assembliesDir;
        }
        if (ConfigurationController.isAppleMobile()) {
            return program;
        }
        return path.dirname(program);
    }
} 
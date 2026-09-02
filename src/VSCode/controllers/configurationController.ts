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

    public static activate(context: vscode.ExtensionContext) {
        ConfigurationController.androidSdkDirectory = Interop.getAndroidSdk();
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveDeviceName, () => ConfigurationController.device?.name));
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveDeviceSerial, () => ConfigurationController.device?.serial));
    }

    public static isMacCatalyst() { return ConfigurationController.device?.platform === 'maccatalyst'; }
    public static isWindows() { return ConfigurationController.device?.platform === 'windows'; }
    public static isAndroid() { return ConfigurationController.device?.platform === 'android'; }
    public static isAppleMobile() { return ConfigurationController.device?.platform === 'ios'; }

    public static getDebuggingPort(): number {
        if (ConfigurationController.isAndroid())
            return ConfigurationController.getSetting(res.configIdAndroidPort, 0);
        if (ConfigurationController.isAppleMobile())
            return ConfigurationController.getSetting(res.configIdApplePort, 0)
        return 0;
    }
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
            if (bundleName !== undefined)
                return path.join(outDir, bundleName + '.app');
        }

        return targetPath;
    }
    public static getAssetsPath(program: string, project: Project, configuration: string): string | undefined {
        if (ConfigurationController.isAndroid()) {
            const assembliesDir = Interop.getPropertyValue('MonoAndroidIntermediateAssemblyDir', project, configuration, undefined);
            if (assembliesDir === undefined || path.isAbsolute(assembliesDir))
                return assembliesDir;
            return path.join(path.dirname(project.path), assembliesDir);
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
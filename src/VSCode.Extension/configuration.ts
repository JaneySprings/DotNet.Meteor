import { window, workspace, ExtensionContext } from 'vscode';
import { Project, Device, Target } from "./models";
import { CommandController } from './bridge';
import { UIController } from "./controller";
import * as res from './resources';


export class ConfigurationController {
    public static androidSdkDirectory: string | undefined;
    public static project: Project | undefined;
    public static device: Device | undefined;
    public static target: Target | undefined;

    public static activate(context: ExtensionContext) {
        ConfigurationController.androidSdkDirectory = CommandController.androidSdk();
    }

    public static getDebuggingPort(): number {
        if (ConfigurationController.isAndroid()) return ConfigurationController.getSetting(
            res.configIdMonoSdbDebuggerPortAndroid,
            res.configDefaultMonoSdbDebuggerPortAndroid
        );
        if (ConfigurationController.isApple()) return ConfigurationController.getSetting(
            res.configIdMonoSdbDebuggerPortApple,
            res.configDefaultMonoSdbDebuggerPortApple
        );
        if (ConfigurationController.isMacCatalyst() || ConfigurationController.isWindows())
            return 0;

        return -1;
    }
    public static getReloadHostPort(): number {
        return ConfigurationController.getSetting<number>(
            res.configIdHotReloadHostPort, 
            res.configDefaultHotReloadHostPort
        );
    }
    public static getUninstallAppOption(): boolean {
        return ConfigurationController.getSetting<boolean>(
            res.configIdUninstallApplicationBeforeInstalling, 
            res.configDefaultUninstallApplicationBeforeInstalling
        );
    }

    public static isMacCatalyst() { return ConfigurationController.device?.platform === 'maccatalyst'; }
    public static isWindows() { return ConfigurationController.device?.platform === 'windows'; }
    public static isAndroid() { return ConfigurationController.device?.platform === 'android'; }
    public static isApple() { return ConfigurationController.device?.platform === 'ios'; }

    public static isValid(): boolean {
        if (!ConfigurationController.project?.path) {
            window.showErrorMessage(res.messageNoProjectFound);
            return false;
        }
        if (!ConfigurationController.device?.platform) {
            window.showErrorMessage(res.messageNoDeviceFound);
            return false;
        }
        if (!UIController.devices.some(it => it.name === ConfigurationController.device?.name)) {
            window.showErrorMessage(res.messageDeviceNotExists);
            return false;
        }
        if (!ConfigurationController.project.frameworks.some(it => it.includes(ConfigurationController.device!.platform!))) {
            window.showErrorMessage(res.messageNoFrameworkFound);
            return false;
        }

        return true;
    }
    public static isActive(): boolean {
        return ConfigurationController.project !== undefined && ConfigurationController.device !== undefined;
    }

    private static getSetting<TResult>(id: string, fallback: TResult): TResult {
        return workspace.getConfiguration(res.configId).get(id) ?? fallback;
    }
} 
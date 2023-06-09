import { Project, Device, Target } from "./models"
import { window, workspace } from 'vscode';
import { UIController } from "./controller";
import * as res from './resources';


export class Configuration {
    public static project: Project | undefined;
    public static device: Device | undefined;
    public static target: Target | undefined;

    public static getSetting<TResult>(id: string, fallback: TResult): TResult {
        return workspace.getConfiguration(res.configId).get(id) ?? fallback;
    }
    public static getDebuggingPort(): number {
        if (Configuration.isAndroid()) return Configuration.getSetting(
            res.configIdMonoSdbDebuggerPortAndroid,
            res.configDefaultMonoSdbDebuggerPortAndroid
        );
        if (Configuration.isApple()) return Configuration.getSetting(
            res.configIdMonoSdbDebuggerPortApple,
            res.configDefaultMonoSdbDebuggerPortApple
        );
        if (Configuration.isMacCatalyst() || Configuration.isWindows())
            return 0;

        return -1;
    }
    public static getReloadHostPort(): number {
        return Configuration.getSetting<number>(
            res.configIdHotReloadHostPort, 
            res.configDefaultHotReloadHostPort
        );
    }
    public static getUninstallAppOption(): boolean {
        return Configuration.getSetting<boolean>(
            res.configIdUninstallApplicationBeforeInstalling, 
            res.configDefaultUninstallApplicationBeforeInstalling
        );
    }

    public static isAndroid() { return Configuration.device?.platform?.includes('android') ?? false; }
    public static isApple() { return Configuration.device?.platform?.includes('ios') ?? false; }
    public static isMacCatalyst() { return Configuration.device?.platform?.includes('maccatalyst') ?? false; }
    public static isWindows() { return Configuration.device?.platform?.includes('windows') ?? false; }


    public static validate(): boolean {
        if (!Configuration.project || !Configuration.project.path) {
            window.showErrorMessage(res.messageNoProjectFound);
            return false;
        }
        if (!Configuration.device || !Configuration.device.platform) {
            window.showErrorMessage(res.messageNoDeviceFound);
            return false;
        }
        if (!UIController.devices.some(it => it.name === Configuration.device?.name)) {
            window.showErrorMessage(res.messageDeviceNotExists);
            return false;
        }

        return true;
    }
} 
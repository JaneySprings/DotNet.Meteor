import { ConfigurationController } from './configurationController';
import { StatusBarController } from './statusbarController';
import { ExtensionContext } from 'vscode';
import { Device } from '../models/device';

export class StateController {
    private static context: ExtensionContext | undefined;

    public static activate(context: ExtensionContext) {
        StateController.context = context;
    }
    public static deactivate() {
        StateController.context = undefined;
    }

    public static saveDevice() {
        if (StateController.context !== undefined)
            StateController.context.workspaceState.update('device', StateController.getDeviceId(ConfigurationController.device));
    }
    public static getDevice(): Device | undefined {
        if (StateController.context === undefined)
            return undefined;

        const device = StateController.context.workspaceState.get<string>('device');
        return StatusBarController.devices.find(it => StateController.getDeviceId(it) === device);
    }

    public static getGlobal<TValue>(key: string): TValue | undefined {
        return StateController.context?.globalState.get<TValue>(key);
    }
    public static putGlobal(key: string, value: any) {
        StateController.context?.globalState.update(key, value);
    }

    private static getDeviceId(device: Device | undefined): string {
        return device ? `${device.name}_${device.platform}_${device.os_version}` : 'null';
    }
}
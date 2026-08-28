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

    public static saveDevice(device: Device, framework: string | undefined) {
        if (StateController.context !== undefined)
            StateController.context.workspaceState.update(`device_${framework}`, StateController.toDeviceId(device));
    }
    public static getDevice(devices: Device[], framework: string | undefined): Device | undefined {
        if (StateController.context === undefined)
            return undefined;

        const deviceId = StateController.context.workspaceState.get<string>(`device_${framework}`);
        return devices.find(d => StateController.toDeviceId(d) === deviceId);
    }

    // public static getGlobal<TValue>(key: string): TValue | undefined {
    //     return StateController.context?.globalState.get<TValue>(key);
    // }
    // public static putGlobal(key: string, value: any) {
    //     StateController.context?.globalState.update(key, value);
    // }

    private static toDeviceId(device: Device | undefined): string {
        return device ? `${device.name}_${device.platform}_${device.os_version}` : 'null';
    }
}
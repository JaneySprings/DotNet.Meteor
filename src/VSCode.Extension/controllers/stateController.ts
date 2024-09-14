import { ConfigurationController } from './configurationController';
import { StatusBarController } from './statusbarController';
import { ExtensionContext } from 'vscode';
import { Target } from '../models/target';
import { Device } from '../models/device';

export class StateController {
    private static context: ExtensionContext | undefined;

    public static activate(context: ExtensionContext) {
        StateController.context = context;
    }
    public static deactivate() {
        StateController.context = undefined;
    }

    public static load() {
        if (StateController.context === undefined)
            return;

        const project = StateController.context.workspaceState.get<string>('project');
        const device = StateController.context.workspaceState.get<string>('device');
        const target = StateController.context.workspaceState.get<Target>('target');

        ConfigurationController.device = StatusBarController.devices.find(it => StateController.getDeviceId(it) === device);
        ConfigurationController.project = StatusBarController.projects.find(it => it.path === project);
        ConfigurationController.target = target;
    }
    public static saveProject() {
        if (StateController.context !== undefined)
             StateController.context.workspaceState.update('project', ConfigurationController.project?.path);
    }
    public static saveDevice() {
        if (StateController.context !== undefined)
            StateController.context.workspaceState.update('device', StateController.getDeviceId(ConfigurationController.device));
    }
    public static saveTarget() {
        if (StateController.context !== undefined)
            StateController.context.workspaceState.update('target', ConfigurationController.target);
    }

    private static getDeviceId(device: Device | undefined): string {
        return device ? `${device.name}_${device.platform}_${device.os_version}` : 'null';
    }
}
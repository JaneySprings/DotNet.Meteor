import { ConfigurationController } from './configurationController';
import { StateController } from './stateController';
import { Device, DeviceItem } from '../models/device';
import { Project } from '../models/project';
import { Interop } from "../interop/interop";
import { Icons } from '../resources/icons';
import * as res from '../resources/constants';
import * as vscode from 'vscode';

export class StatusBarController {
    private static deviceStatusItem: vscode.StatusBarItem | undefined;
    private static devices: Device[];

    public static async activate(context: vscode.ExtensionContext): Promise<void> {
        const exports = await vscode.extensions.getExtension(res.dotrushExtensionId)?.activate();
        exports?.onActiveProjectChanged?.add((p: Project) => ConfigurationController.project = p);
        exports?.onActiveConfigurationChanged?.add((c: string) => ConfigurationController.configuration = c);
        exports?.onActiveFrameworkChanged?.add((f: string) => {
            ConfigurationController.targetFramework = f;
            StatusBarController.performSelectDevice(undefined); // Autoselect best candidate
        });

        StatusBarController.deviceStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 80);
        StatusBarController.deviceStatusItem.command = res.commandIdSelectActiveDevice;
        context.subscriptions.push(StatusBarController.deviceStatusItem);
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveDevice, StatusBarController.showQuickPickDevice));

        await StatusBarController.fetchAllDevices();
        if (StatusBarController.devices.length !== 0)
            StatusBarController.performSelectDevice(undefined);
    }

    private static performSelectDevice(item: Device | undefined = undefined) {
        if (StatusBarController.devices === undefined)
            return;

        item ??= StateController.getDevice(StatusBarController.devices, ConfigurationController.targetFramework);
        if (item === undefined) {
            const filteredDevices = StatusBarController.filterDevices(StatusBarController.devices);
            if (filteredDevices.length !== 0)
                item = filteredDevices[0];
        }

        ConfigurationController.device = item;
        if (ConfigurationController.device === undefined) {
            StatusBarController.deviceStatusItem?.hide();
            return;
        }
        if (StatusBarController.deviceStatusItem !== undefined) {
            StatusBarController.deviceStatusItem.text = `${Icons.deviceKind(ConfigurationController.device)} ${ConfigurationController.device?.name}`;
            StatusBarController.deviceStatusItem.show();
        }
        StateController.saveDevice(ConfigurationController.device, ConfigurationController.targetFramework);
    }
    private static async showQuickPickDevice() {
        const picker = vscode.window.createQuickPick();
        picker.placeholder = res.messageDeviceLoading;
        picker.matchOnDetail = true;
        picker.busy = true;
        picker.show();
        picker.onDidAccept(() => {
            if (picker.selectedItems !== undefined) {
                const selectedItem = (picker.selectedItems[0] as DeviceItem).item;
                StatusBarController.performSelectDevice(selectedItem);
            }
            picker.hide();
        });

        const devices = await StatusBarController.fetchAllDevices();
        picker.items = StatusBarController.filterDevices(devices).map(d => new DeviceItem(d));
        picker.placeholder = `${ConfigurationController.project?.name} (${ConfigurationController.targetFramework}): ${res.commandTitleSelectActiveDevice}`
        picker.busy = false;
    }

    private static filterDevices(devices: Device[]): Device[] {
        if (ConfigurationController.targetFramework === undefined)
            return [];

        return devices.filter(d => ConfigurationController.targetFramework?.includes(d.platform.toLocaleLowerCase()))
    }
    private static async fetchAllDevices(): Promise<Device[]> {
        StatusBarController.devices = await Interop.getDevices();
        return StatusBarController.devices;
    }
}
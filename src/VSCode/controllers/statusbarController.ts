import { ConfigurationController } from './configurationController';
import { StateController } from './stateController';
import { Device, DeviceItem, SeparatorItem } from '../models/device';
import { Project } from '../models/project';
import { Interop } from "../interop/interop";
import { Icons } from '../resources/icons';
import * as res from '../resources/constants';
import * as vscode from 'vscode';

export class StatusBarController {
    private static deviceStatusItem: vscode.StatusBarItem | undefined;
    public static devices: Device[];

    public static async activate(context: vscode.ExtensionContext): Promise<void> {
        const exports = await vscode.extensions.getExtension(res.dotrushExtensionId)?.activate();
        exports?.onActiveProjectChanged?.add((p: Project) => ConfigurationController.project = p);
        exports?.onActiveConfigurationChanged?.add((c: string) => ConfigurationController.configuration = c);
        exports?.onActiveFrameworkChanged?.add((f: string) => ConfigurationController.targetFramework = f);

        StatusBarController.createDeviceStatusBarItem(context);
        StatusBarController.updateDeviceStatusBarItem();
    }

    private static createDeviceStatusBarItem(context: vscode.ExtensionContext) {
        StatusBarController.deviceStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 80);
        StatusBarController.deviceStatusItem.command = res.commandIdSelectActiveDevice;
        context.subscriptions.push(StatusBarController.deviceStatusItem);
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveDevice, StatusBarController.showQuickPickDevice));
    }
    private static async updateDeviceStatusBarItem(): Promise<void> {
        if (StatusBarController.deviceStatusItem === undefined)
            return;

        StatusBarController.devices = await Interop.getDevices();
        if (StatusBarController.devices.length === 0)
            return StatusBarController.deviceStatusItem.hide();

        StatusBarController.performSelectDevice(StateController.getDevice());
        StatusBarController.deviceStatusItem.show();
    }

    public static performSelectDevice(item: Device | undefined = undefined) {
        ConfigurationController.device = item ?? StatusBarController.devices[0];
        if (StatusBarController.deviceStatusItem !== undefined)
            StatusBarController.deviceStatusItem.text = `${Icons.deviceKind(ConfigurationController.device)} ${ConfigurationController.device?.name}`;
        StateController.saveDevice();
    }
    public static async showQuickPickDevice() {
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

        StatusBarController.devices = await Interop.getDevices();

        const items: vscode.QuickPickItem[] = [];
        for (let i of StatusBarController.devices.keys()) {
            if (i == 0 || StatusBarController.devices[i].category !== StatusBarController.devices[i - 1].category)
                items.push(new SeparatorItem(StatusBarController.devices[i].category));

            items.push(new DeviceItem(StatusBarController.devices[i]));
        }

        picker.items = items;
        picker.placeholder = res.commandTitleSelectActiveDevice;
        picker.busy = false;
    }
}
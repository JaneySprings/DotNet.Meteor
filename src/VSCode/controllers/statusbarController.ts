import { ConfigurationController } from './configurationController';
import { Interop } from "../interop/interop";
import { StateController } from './stateController';
import { Project } from '../models/project';
import { ProjectItem } from '../models/projectItem';
import { Device } from '../models/device';
import { DeviceItem } from '../models/deviceItem';
import { SeparatorItem } from '../models/separatorItem';
import { Icons } from '../resources/icons';
import * as res from '../resources/constants';
import * as vscode from 'vscode';

export class StatusBarController {
    private static projectStatusItem: vscode.StatusBarItem | undefined;
    private static targetStatusItem: vscode.StatusBarItem | undefined;
    private static deviceStatusItem: vscode.StatusBarItem | undefined;

    public static projects: Project[];
    public static devices: Device[];

    public static async activate(context: vscode.ExtensionContext): Promise<void> {
        if (vscode.extensions.getExtension(res.dotrushExtensionId) !== undefined)
            return StatusBarController.activateWithDotRush(context);

        StatusBarController.createProjectStatusBarItem(context);
        StatusBarController.createConfigurationStatusBarItem(context);
        StatusBarController.createDeviceStatusBarItem(context);

        context.subscriptions.push(vscode.workspace.onDidChangeWorkspaceFolders(_ => {
            StatusBarController.updateProjectStatusBarItem();
        }));
        context.subscriptions.push(vscode.workspace.onDidSaveTextDocument(ev => {
            if (ev.fileName.endsWith('proj') || ev.fileName.endsWith('.props'))
                StatusBarController.updateProjectStatusBarItem();
        }));

        StatusBarController.updateProjectStatusBarItem();
        StatusBarController.updateDeviceStatusBarItem();
    }
    public static async activateWithDotRush(context: vscode.ExtensionContext): Promise<void> {
        const exports = await vscode.extensions.getExtension(res.dotrushExtensionId)?.activate();
        exports?.onActiveProjectChanged?.add((p: Project) => ConfigurationController.project = p);
        exports?.onActiveConfigurationChanged?.add((c: string) => ConfigurationController.configuration = c);

        StatusBarController.createDeviceStatusBarItem(context);
        StatusBarController.updateDeviceStatusBarItem();
    }

    private static createProjectStatusBarItem(context: vscode.ExtensionContext) {
        StatusBarController.projectStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 100);
        StatusBarController.projectStatusItem.command = res.commandIdSelectActiveProject;
        context.subscriptions.push(StatusBarController.projectStatusItem);
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveProject, StatusBarController.showQuickPickProject));
    }
    private static createConfigurationStatusBarItem(context: vscode.ExtensionContext) {
        StatusBarController.targetStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 90);
        StatusBarController.targetStatusItem.command = res.commandIdSelectActiveConfiguration;
        context.subscriptions.push(StatusBarController.targetStatusItem);
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveConfiguration, StatusBarController.showQuickPickConfiguration));
    }
    private static createDeviceStatusBarItem(context: vscode.ExtensionContext) {
        StatusBarController.deviceStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 80);
        StatusBarController.deviceStatusItem.command = res.commandIdSelectActiveDevice;
        context.subscriptions.push(StatusBarController.deviceStatusItem);
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveDevice, StatusBarController.showQuickPickDevice));
    }

    private static async updateProjectStatusBarItem(): Promise<void> {
        if (StatusBarController.projectStatusItem === undefined)
            return;

        const folders = vscode.workspace.workspaceFolders!.map(it => it.uri.fsPath);
        StatusBarController.projects = await Interop.getProjects(folders);
        if (StatusBarController.projects.length === 0)
            return StatusBarController.projectStatusItem.hide();

        StatusBarController.performSelectProject(StateController.getProject());
        StatusBarController.projects.length === 1
            ? StatusBarController.projectStatusItem.hide()
            : StatusBarController.projectStatusItem.show();

        StatusBarController.updateConfigurationStatusBarItem();
    }
    private static async updateConfigurationStatusBarItem(): Promise<void> {
        if (StatusBarController.targetStatusItem === undefined)
            return;

        if (StatusBarController.projects.length === 0)
            return StatusBarController.targetStatusItem.hide();

        StatusBarController.performSelectConfiguration(StateController.getConfiguration());
        StatusBarController.targetStatusItem.show();
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

    public static performSelectProject(item: Project | undefined = undefined) {
        ConfigurationController.project = item ?? StatusBarController.projects[0];
        if (StatusBarController.projectStatusItem !== undefined)
            StatusBarController.projectStatusItem.text = `${Icons.project} ${ConfigurationController.project?.name}`;
        StateController.saveProject();
    }
    public static performSelectConfiguration(item: string | undefined = undefined) {
        ConfigurationController.configuration = item ?? 'Debug';
        if (StatusBarController.targetStatusItem !== undefined)
            StatusBarController.targetStatusItem.text = `${Icons.target} ${ConfigurationController.configuration} | Any CPU`;
        StateController.saveConfiguration();
    }
    public static performSelectDevice(item: Device | undefined = undefined) {
        ConfigurationController.device = item ?? StatusBarController.devices[0];
        if (StatusBarController.deviceStatusItem !== undefined)
            StatusBarController.deviceStatusItem.text = `${Icons.deviceKind(ConfigurationController.device)} ${ConfigurationController.device?.name}`;
        StateController.saveDevice();
    }

    public static async showQuickPickProject() {
        const items = StatusBarController.projects.map(project => new ProjectItem(project));
        const options = { placeHolder: res.commandTitleSelectActiveProject };
        const selectedItem = await vscode.window.showQuickPick(items, options);

        if (selectedItem !== undefined) {
            StatusBarController.performSelectProject(selectedItem.item);
            StatusBarController.performSelectConfiguration(undefined);
        }
    }
    public static async showQuickPickConfiguration() {
        const items = ConfigurationController.project?.configurations ?? [];
        const options = { placeHolder: res.commandTitleSelectActiveConfiguration };
        const selectedItem = await vscode.window.showQuickPick(items, options);

        if (selectedItem !== undefined)
            StatusBarController.performSelectConfiguration(selectedItem);
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
            if (i == 0 || StatusBarController.devices[i].detail !== StatusBarController.devices[i - 1].detail)
                items.push(new SeparatorItem(StatusBarController.devices[i].detail));

            items.push(new DeviceItem(StatusBarController.devices[i]));
        }

        picker.items = items;
        picker.placeholder = res.commandTitleSelectActiveDevice;
        picker.busy = false;
    }
}
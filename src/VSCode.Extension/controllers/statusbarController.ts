import { ConfigurationController } from './configurationController';
import { StateController } from './stateController';
import { Interop } from "../interop/interop";
import { Project } from '../models/project';
import { ProjectItem } from '../models/projectItem';
import { Device } from '../models/device';
import { DeviceItem } from '../models/deviceItem';
import { SeparatorItem } from '../models/separatorItem';
import { Icons } from '../resources/icons';
import * as res from '../resources/constants';
import * as vscode from 'vscode';

export class StatusBarController {
    private static projectStatusItem: vscode.StatusBarItem;
    private static targetStatusItem: vscode.StatusBarItem;
    private static deviceStatusItem: vscode.StatusBarItem;

    public static projects: Project[];
    public static devices: Device[];

    private static dotrushExports: any | undefined;

    public static async activate(context: vscode.ExtensionContext): Promise<void> {
        StatusBarController.projectStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 100);
        StatusBarController.targetStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 99);
        StatusBarController.deviceStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 98);

        StatusBarController.targetStatusItem.command = res.commandIdSelectActiveConfiguration;
        StatusBarController.projectStatusItem.command = res.commandIdSelectActiveProject;
        StatusBarController.deviceStatusItem.command = res.commandIdSelectActiveDevice;

        context.subscriptions.push(StatusBarController.projectStatusItem);
        context.subscriptions.push(StatusBarController.targetStatusItem);
        context.subscriptions.push(StatusBarController.deviceStatusItem);

        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveProject, StatusBarController.showQuickPickProject));
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveConfiguration, StatusBarController.showQuickPickConfiguration));
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveDevice, StatusBarController.showQuickPickDevice));
        
        if (vscode.extensions.getExtension(res.dotrushExtensionId) !== undefined)
            StatusBarController.dotrushExports = await vscode.extensions.getExtension(res.dotrushExtensionId)?.activate();
        
        if (StatusBarController.dotrushExports === undefined) {
            context.subscriptions.push(vscode.workspace.onDidChangeWorkspaceFolders(() => StatusBarController.update()));
            context.subscriptions.push(vscode.workspace.onDidSaveTextDocument(ev => {
                if (ev.fileName.endsWith('proj') || ev.fileName.endsWith('.props'))
                    StatusBarController.update();
            }));

            StatusBarController.update();
        } else {
            StatusBarController.dotrushExports.onProjectsChanged.add(StatusBarController.update);
            StatusBarController.dotrushExports.onActiveProjectChanged.add(StatusBarController.performSelectProject);
            StatusBarController.dotrushExports.onActiveConfigurationChanged.add(StatusBarController.performSelectConfiguration);
        }
    }
    public static deactivate() {
        StatusBarController.dotrushExports?.onProjectsChanged?.remove(StatusBarController.update);
        StatusBarController.dotrushExports?.onActiveProjectChanged?.remove(StatusBarController.performSelectProject);
        StatusBarController.dotrushExports?.onActiveConfigurationChanged?.remove(StatusBarController.performSelectConfiguration);
    }

    public static async update(projects: Project[] | undefined = undefined, devices: Device[] | undefined = undefined) : Promise<void> {
        const folders = vscode.workspace.workspaceFolders!.map(it => it.uri.fsPath);
        StatusBarController.projects = projects ?? await Interop.getProjects(folders);
        StatusBarController.devices = devices ?? await Interop.getDevices();

        if (StatusBarController.projects.length === 0 || StatusBarController.devices.length === 0) {
            StatusBarController.projectStatusItem.hide();
            StatusBarController.targetStatusItem.hide();
            StatusBarController.deviceStatusItem.hide();
            return;
        }
        
        StateController.load();
        StatusBarController.performSelectProject(ConfigurationController.project);
        StatusBarController.performSelectConfiguration(ConfigurationController.configuration);
        StatusBarController.performSelectDevice(ConfigurationController.device);

        if (StatusBarController.dotrushExports !== undefined) {
            StatusBarController.deviceStatusItem.show();
            return;
        }
        
        StatusBarController.deviceStatusItem.show();
        StatusBarController.targetStatusItem.show();
        StatusBarController.projects.length === 1 
            ? StatusBarController.projectStatusItem.hide() 
            : StatusBarController.projectStatusItem.show();
    }

    public static performSelectProject(item: Project | undefined = undefined) {
        ConfigurationController.project = item ?? StatusBarController.projects[0];
        StatusBarController.projectStatusItem.text = `${Icons.project} ${ConfigurationController.project?.name}`;
        StateController.saveProject();
    }
    public static performSelectConfiguration(item: string | undefined = undefined) {
        ConfigurationController.configuration = item ?? 'Debug';
        StatusBarController.targetStatusItem.text = `${Icons.target} ${ConfigurationController.configuration} | Any CPU`;
        StateController.saveTarget();
    }
    public static performSelectDevice(item: Device | undefined = undefined) {
        ConfigurationController.device = item ?? StatusBarController.devices[0];
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
            if (i == 0 || StatusBarController.devices[i].detail !== StatusBarController.devices[i-1].detail) 
                items.push(new SeparatorItem(StatusBarController.devices[i].detail));

            items.push(new DeviceItem(StatusBarController.devices[i]));    
        }
        
        picker.items = items;
        picker.placeholder = res.commandTitleSelectActiveDevice;
        picker.busy = false;
    }
}
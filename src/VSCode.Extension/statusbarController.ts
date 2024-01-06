import { ConfigurationController } from './configurationController';
import { CommandController } from "./commandController";
import { StateController } from './stateController';
import { XamlController } from './xaml/xamlController';
import { PublicExports } from './publicExports';
import { Project } from './models/project';
import { ProjectItem } from './models/projectItem';
import { Device } from './models/device';
import { DeviceItem } from './models/deviceItem';
import { Target } from './models/target';
import { SeparatorItem } from './models/separatorItem';
import { Icons } from './resources/icons';
import * as res from './resources/constants';
import * as vscode from 'vscode';

export class StatusBarController {
    private static projectStatusItem: vscode.StatusBarItem;
    private static targetStatusItem: vscode.StatusBarItem;
    private static deviceStatusItem: vscode.StatusBarItem;

    public static projects: Project[];
    public static devices: Device[];

    public static activate(context: vscode.ExtensionContext) {
        StatusBarController.projectStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 100);
        StatusBarController.targetStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 90);
        StatusBarController.deviceStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 80);

        StatusBarController.targetStatusItem.command = res.commandIdSelectActiveConfiguration;
        StatusBarController.projectStatusItem.command = res.commandIdSelectActiveProject;
        StatusBarController.deviceStatusItem.command = res.commandIdSelectActiveDevice;

        context.subscriptions.push(StatusBarController.projectStatusItem);
        context.subscriptions.push(StatusBarController.targetStatusItem);
        context.subscriptions.push(StatusBarController.deviceStatusItem);
    }
    public static async update() {
        const folders = vscode.workspace.workspaceFolders!.map(it => it.uri.fsPath);
        StatusBarController.projects = await CommandController.getProjects(folders);
        StatusBarController.devices = await CommandController.getDevices();

        if (StatusBarController.projects.length === 0 || StatusBarController.devices.length === 0) {
            StatusBarController.projectStatusItem.hide();
            StatusBarController.targetStatusItem.hide();
            StatusBarController.deviceStatusItem.hide();
            PublicExports.instance.invokeAll();
            return;
        }
        
        StateController.load();
        StatusBarController.performSelectProject(ConfigurationController.project);
        StatusBarController.performSelectTarget(ConfigurationController.target);
        StatusBarController.performSelectDevice(ConfigurationController.device);

        StatusBarController.targetStatusItem.show();
        StatusBarController.deviceStatusItem.show();
        StatusBarController.projects.length === 1 
            ? StatusBarController.projectStatusItem.hide() 
            : StatusBarController.projectStatusItem.show();
    }

    public static performSelectProject(item: Project | undefined = undefined) {
        ConfigurationController.project = item ?? StatusBarController.projects[0];
        StatusBarController.projectStatusItem.text = `${Icons.project} ${ConfigurationController.project?.name}`;
        PublicExports.instance.projectChangedEventHandler.invoke(ConfigurationController.project);
        XamlController.regenerate();
        StateController.saveProject();
    }
    public static performSelectTarget(item: Target | undefined = undefined) {
        ConfigurationController.target = item ?? Target.Debug;
        StatusBarController.targetStatusItem.text = `${Icons.target} ${ConfigurationController.target} | Any CPU`;
        PublicExports.instance.targetChangedEventHandler.invoke(ConfigurationController.target);
        StateController.saveTarget();
    }
    public static performSelectDevice(item: Device | undefined = undefined) {
        ConfigurationController.device = item ?? StatusBarController.devices[0];
        StatusBarController.deviceStatusItem.text = `${Icons.deviceKind(ConfigurationController.device)} ${ConfigurationController.device?.name}`;
        PublicExports.instance.deviceChangedEventHandler.invoke(ConfigurationController.device);
        StateController.saveDevice();
    }

    public static async showQuickPickProject() {
        const items = StatusBarController.projects.map(project => new ProjectItem(project));
        const options = { placeHolder: res.commandTitleSelectActiveProject };
        const selectedItem = await vscode.window.showQuickPick(items, options);

        if (selectedItem !== undefined) 
            StatusBarController.performSelectProject(selectedItem.item);
    }
    public static async showQuickPickTarget() {
        const items = [ Target.Debug, Target.Release ];
        const options = { placeHolder: res.commandTitleSelectActiveConfiguration };
        const selectedItem = await vscode.window.showQuickPick(items, options);
        
        if (selectedItem !== undefined)
            StatusBarController.performSelectTarget(selectedItem as Target);
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

        StatusBarController.devices = await CommandController.getDevices();

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
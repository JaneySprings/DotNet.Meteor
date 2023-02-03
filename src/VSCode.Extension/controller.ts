import { Target, Project, ProjectItem, Device, DeviceItem, Icon } from "./models"
import { Configuration } from './configuration';
import { CommandLine } from "./bridge";
import * as res from './resources';
import * as vscode from 'vscode';


export class UIController {
    public static projectStatusItem: vscode.StatusBarItem;
    public static targetStatusItem: vscode.StatusBarItem;
    public static deviceStatusItem: vscode.StatusBarItem;
    public static workspaceProjects: Project[];
    public static mobileDevices: Device[];

    
    public static activate(context: vscode.ExtensionContext) {
        this.projectStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 100);
        this.targetStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 90);
        this.deviceStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 80);

        this.targetStatusItem.command = res.commandIdSelectActiveConfiguration;
        this.projectStatusItem.command = res.commandIdSelectActiveProject;
        this.deviceStatusItem.command = res.commandIdSelectActiveDevice;

        context.subscriptions.push(this.projectStatusItem);
        context.subscriptions.push(this.targetStatusItem);
        context.subscriptions.push(this.deviceStatusItem);
    }

    public static performSelectProject(item: Project) {
        Configuration.selectedProject = item;
        this.projectStatusItem.text = `${Icon.project} ${Configuration.selectedProject?.name}`;
        UIController.workspaceProjects.length === 1 ? this.projectStatusItem.hide() : this.projectStatusItem.show();
    }
    public static performSelectTarget(target: Target) {
        Configuration.selectedTarget = target;
        this.targetStatusItem.text = `${Icon.target} ${Configuration.selectedTarget} | Any CPU`;
        this.targetStatusItem.show();
    }
    public static performSelectDevice(item: Device) {
        Configuration.selectedDevice = item;
        const icon = item.is_mobile ? Icon.device : Icon.computer;
        this.deviceStatusItem.text = `${icon} ${Configuration.selectedDevice?.name}`;
        this.deviceStatusItem.show();
    }


    public static async showQuickPickProject() {
        const selectedItem = await vscode.window.showQuickPick(
            UIController.workspaceProjects.map(project => new ProjectItem(project)), 
            { placeHolder: res.commandTitleSelectActiveProject }
        );

        if (selectedItem !== undefined) 
        UIController.performSelectProject(selectedItem.item);
    }
    public static async showQuickPickTarget() {
        const selectedItem = await vscode.window.showQuickPick(
            [ Target.Debug, Target.Release ], 
            { placeHolder: res.commandTitleSelectActiveConfiguration }
        );
        
        if (selectedItem !== undefined)
            UIController.performSelectTarget(selectedItem as Target);
    }
    public static async showQuickPickDevice() {
        const picker = vscode.window.createQuickPick();
        picker.placeholder = "Fetching devices...";
        picker.canSelectMany = false;
        picker.busy = true;
        picker.show();

        picker.onDidAccept(() => {
            if (picker.selectedItems !== undefined) {
                const selectedItem = (picker.selectedItems[0] as DeviceItem).item;
                UIController.performSelectDevice(selectedItem);
            }
            picker.hide();
        });

        CommandLine.mobileDevicesAsync(items => {
            UIController.mobileDevices = items;
            picker.items = items.map(device => new DeviceItem(device));
            picker.placeholder = res.commandTitleSelectActiveDevice;
            picker.busy = false;
        });
    }
}
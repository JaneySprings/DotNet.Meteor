import { Configuration } from './configuration';
import { CommandInterface } from "./bridge";
import * as res from './resources';
import * as models from "./models"
import * as vscode from 'vscode';


export class UIController {
    public static projectStatusItem: vscode.StatusBarItem;
    public static targetStatusItem: vscode.StatusBarItem;
    public static deviceStatusItem: vscode.StatusBarItem;
    public static workspaceProjects: models.Project[];
    public static mobileDevices: models.Device[];

    private static _isActivated: boolean = true;

    
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

    public static deactivate() {
        this.projectStatusItem.hide();
        this.targetStatusItem.hide();
        this.deviceStatusItem.hide();
        this._isActivated = false;
    }

    public static performSelectProject(item: models.Project | undefined = undefined) {
        if (!this._isActivated) return;
        Configuration.selectedProject = item ?? UIController.workspaceProjects[0];
        this.projectStatusItem.text = `${models.Icon.project} ${Configuration.selectedProject?.name}`;
        UIController.workspaceProjects.length === 1 ? this.projectStatusItem.hide() : this.projectStatusItem.show();
    }
    public static performSelectTarget(target: models.Target | undefined = undefined) {
        if (!this._isActivated) return;
        Configuration.selectedTarget = target ?? models.Target.Debug;
        this.targetStatusItem.text = `${models.Icon.target} ${Configuration.selectedTarget} | Any CPU`;
        this.targetStatusItem.show();
    }
    public static performSelectDevice(item: models.Device | undefined = undefined) {
        if (!this._isActivated) return;
        Configuration.selectedDevice = item ?? UIController.mobileDevices[0];
        const icon = Configuration.selectedDevice.is_mobile ? models.Icon.device : models.Icon.computer;
        this.deviceStatusItem.text = `${icon} ${Configuration.selectedDevice?.name}`;
        this.deviceStatusItem.show();
    }


    public static async showQuickPickProject() {
        const items = UIController.workspaceProjects.map(project => new models.ProjectItem(project));
        const options = { placeHolder: res.commandTitleSelectActiveProject };
        const selectedItem = await vscode.window.showQuickPick(items, options);

        if (selectedItem !== undefined) 
            UIController.performSelectProject(selectedItem.item);
    }
    public static async showQuickPickTarget() {
        const items = [ models.Target.Debug, models.Target.Release ];
        const options = { placeHolder: res.commandTitleSelectActiveConfiguration };
        const selectedItem = await vscode.window.showQuickPick(items, options);
        
        if (selectedItem !== undefined)
            UIController.performSelectTarget(selectedItem as models.Target);
    }
    public static async showQuickPickDevice() {
        const picker = vscode.window.createQuickPick();
        picker.placeholder = "Fetching devices...";
        picker.canSelectMany = false;
        picker.busy = true;
        picker.show();
        picker.onDidAccept(() => {
            if (picker.selectedItems !== undefined) {
                const selectedItem = (picker.selectedItems[0] as models.DeviceItem).item;
                UIController.performSelectDevice(selectedItem);
            }
            picker.hide();
        });

        CommandInterface.mobileDevicesAsync(items => {
            UIController.mobileDevices = items;
            picker.items = items.map(device => new models.DeviceItem(device));
            picker.placeholder = res.commandTitleSelectActiveDevice;
            picker.busy = false;
        });
    }
}
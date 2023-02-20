import { Configuration } from './configuration';
import { CommandInterface } from "./bridge";
import * as res from './resources';
import * as models from "./models"
import * as vscode from 'vscode';
import { SchemaController } from './xaml/schemacontroller';


export class UIController {
    private static _projectStatusItem: vscode.StatusBarItem;
    private static _targetStatusItem: vscode.StatusBarItem;
    private static _deviceStatusItem: vscode.StatusBarItem;

    public static projects: models.Project[];
    public static devices: models.Device[];

    
    public static activate(context: vscode.ExtensionContext) {
        UIController._projectStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 100);
        UIController._targetStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 90);
        UIController._deviceStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 80);

        UIController._targetStatusItem.command = res.commandIdSelectActiveConfiguration;
        UIController._projectStatusItem.command = res.commandIdSelectActiveProject;
        UIController._deviceStatusItem.command = res.commandIdSelectActiveDevice;

        context.subscriptions.push(UIController._projectStatusItem);
        context.subscriptions.push(UIController._targetStatusItem);
        context.subscriptions.push(UIController._deviceStatusItem);
    }
    public static show() {
        UIController._projectStatusItem.show();
        UIController._targetStatusItem.show();
        UIController._deviceStatusItem.show();
    }
    public static hide() {
        UIController._projectStatusItem.hide();
        UIController._targetStatusItem.hide();
        UIController._deviceStatusItem.hide();
    }

    public static performSelectProject(item: models.Project | undefined = undefined) {
        Configuration.project = item ?? UIController.projects[0];
        UIController._projectStatusItem.text = `${models.Icon.project} ${Configuration.project?.name}`;
        UIController.projects.length === 1 ? UIController._projectStatusItem.hide() : UIController._projectStatusItem.show();
        SchemaController.invalidate();
    }
    public static performSelectTarget(item: models.Target | undefined = undefined) {
        Configuration.target = item ?? models.Target.Debug;
        UIController._targetStatusItem.text = `${models.Icon.target} ${Configuration.target} | Any CPU`;
    }
    public static performSelectDevice(item: models.Device | undefined = undefined) {
        Configuration.device = item ?? UIController.devices[0];
        const icon = Configuration.device.is_mobile ? models.Icon.device : models.Icon.computer;
        UIController._deviceStatusItem.text = `${icon} ${Configuration.device?.name}`;
    }


    public static async showQuickPickProject() {
        const items = UIController.projects.map(project => new models.ProjectItem(project));
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

        CommandInterface.devicesAsync(items => {
            UIController.devices = items;
            picker.items = items.map(device => new models.DeviceItem(device));
            picker.placeholder = res.commandTitleSelectActiveDevice;
            picker.busy = false;
        });
    }
}
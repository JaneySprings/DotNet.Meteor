import { XamlService } from './xaml/service';
import { Configuration } from './configuration';
import { CommandInterface } from "./bridge";
import { PublicExports } from './exports';
import { StateManager } from './cache';
import * as res from './resources';
import * as models from "./models"
import * as vscode from 'vscode';


export class UIController {
    private static _projectStatusItem: vscode.StatusBarItem;
    private static _targetStatusItem: vscode.StatusBarItem;
    private static _deviceStatusItem: vscode.StatusBarItem;

    public static projects: models.Project[];
    public static devices: models.Device[];


//#region Lifecycle
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

    public static async update() {
        const folders = vscode.workspace.workspaceFolders!.map(it => it.uri.fsPath);
        UIController.projects = await CommandInterface.getProjects(folders);
        UIController.devices = await CommandInterface.getDevices();

        if (UIController.projects.length === 0 || UIController.devices.length === 0) {
            UIController._projectStatusItem.hide();
            UIController._targetStatusItem.hide();
            UIController._deviceStatusItem.hide();
            PublicExports.instance.invokeAll();
            return;
        }
        if (Configuration.project === undefined || Configuration.device === undefined)
            StateManager.load();

        Configuration.project = UIController.projects.find(it => it.name === Configuration.project?.name);
        Configuration.device = UIController.devices.find(it => it.name === Configuration.device?.name);

        UIController.performSelectProject(Configuration.project);
        UIController.performSelectTarget(Configuration.target);
        UIController.performSelectDevice(Configuration.device);

        UIController._targetStatusItem.show();
        UIController._deviceStatusItem.show();
        UIController.projects.length === 1 
            ? UIController._projectStatusItem.hide() 
            : UIController._projectStatusItem.show();
    }
//#endregion

//#region UI Commands
    public static performSelectProject(item: models.Project | undefined = undefined) {
        Configuration.project = item ?? UIController.projects[0];
        UIController._projectStatusItem.text = `${models.Icon.project} ${Configuration.project?.name}`;
        PublicExports.instance.projectChangedEventHandler.invoke(Configuration.project);
        XamlService.regenerate();
        StateManager.saveProject();
    }
    public static performSelectTarget(item: models.Target | undefined = undefined) {
        Configuration.target = item ?? models.Target.Debug;
        UIController._targetStatusItem.text = `${models.Icon.target} ${Configuration.target} | Any CPU`;
        PublicExports.instance.targetChangedEventHandler.invoke(Configuration.target);
        StateManager.saveTarget();
    }
    public static performSelectDevice(item: models.Device | undefined = undefined) {
        Configuration.device = item ?? UIController.devices[0];
        const icon = Configuration.device.is_mobile ? models.Icon.device : models.Icon.computer;
        UIController._deviceStatusItem.text = `${icon} ${Configuration.device?.name}`;
        PublicExports.instance.deviceChangedEventHandler.invoke(Configuration.device);
        StateManager.saveDevice();
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
        picker.placeholder = res.messageDeviceLoading;
        picker.matchOnDetail = true;
        picker.busy = true;
        picker.show();
        picker.onDidAccept(() => {
            if (picker.selectedItems !== undefined) {
                const selectedItem = (picker.selectedItems[0] as models.DeviceItem).item;
                UIController.performSelectDevice(selectedItem);
            }
            picker.hide();
        });

        UIController.devices = await CommandInterface.getDevices();

        const items: vscode.QuickPickItem[] = [];
        for (let i of UIController.devices.keys()) {
            if (i == 0 || UIController.devices[i].detail !== UIController.devices[i-1].detail) 
                items.push(new models.SeparatorItem(UIController.devices[i].detail));

            items.push(new models.DeviceItem(UIController.devices[i]));    
        }
        
        picker.items = items;
        picker.placeholder = res.commandTitleSelectActiveDevice;
        picker.busy = false;
    }
//#endregion
}
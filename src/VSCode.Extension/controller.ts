import { ConfigurationController } from './configuration';
import { XamlController } from './xaml/service';
import { CommandController } from "./bridge";
import { PublicExports } from './exports';
import { StateController } from './cache';
import * as res from './resources';
import * as models from "./models"
import * as vscode from 'vscode';


export class UIController {
    private static _projectStatusItem: vscode.StatusBarItem;
    private static _targetStatusItem: vscode.StatusBarItem;
    private static _deviceStatusItem: vscode.StatusBarItem;

    public static projects: models.IProject[];
    public static devices: models.IDevice[];


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
        UIController.projects = await CommandController.getProjects(folders);
        UIController.devices = await CommandController.getDevices();

        if (UIController.projects.length === 0 || UIController.devices.length === 0) {
            UIController._projectStatusItem.hide();
            UIController._targetStatusItem.hide();
            UIController._deviceStatusItem.hide();
            PublicExports.instance.invokeAll();
            return;
        }
        if (ConfigurationController.project === undefined || ConfigurationController.device === undefined)
            StateController.load();

        ConfigurationController.project = UIController.projects.find(it => it.name === ConfigurationController.project?.name);
        ConfigurationController.device = UIController.devices.find(it => it.name === ConfigurationController.device?.name);

        UIController.performSelectProject(ConfigurationController.project);
        UIController.performSelectTarget(ConfigurationController.target);
        UIController.performSelectDevice(ConfigurationController.device);

        UIController._targetStatusItem.show();
        UIController._deviceStatusItem.show();
        UIController.projects.length === 1 
            ? UIController._projectStatusItem.hide() 
            : UIController._projectStatusItem.show();
    }
//#endregion

//#region UI Commands
    public static performSelectProject(item: models.IProject | undefined = undefined) {
        ConfigurationController.project = item ?? UIController.projects[0];
        UIController._projectStatusItem.text = `${models.Icon.project} ${ConfigurationController.project?.name}`;
        PublicExports.instance.projectChangedEventHandler.invoke(ConfigurationController.project);
        XamlController.regenerate();
        StateController.saveProject();
    }
    public static performSelectTarget(item: models.Target | undefined = undefined) {
        ConfigurationController.target = item ?? models.Target.Debug;
        UIController._targetStatusItem.text = `${models.Icon.target} ${ConfigurationController.target} | Any CPU`;
        PublicExports.instance.targetChangedEventHandler.invoke(ConfigurationController.target);
        StateController.saveTarget();
    }
    public static performSelectDevice(item: models.IDevice | undefined = undefined) {
        ConfigurationController.device = item ?? UIController.devices[0];
        const icon = ConfigurationController.device.is_mobile ? models.Icon.device : models.Icon.computer;
        UIController._deviceStatusItem.text = `${icon} ${ConfigurationController.device?.name}`;
        PublicExports.instance.deviceChangedEventHandler.invoke(ConfigurationController.device);
        StateController.saveDevice();
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

        UIController.devices = await CommandController.getDevices();

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
import * as vscode from 'vscode';
import { Configuration, Target } from './configuration';
import * as constants from './constants';

export class Interface {
    public static readonly projectStatusItem: vscode.StatusBarItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 100);
    public static readonly targetStatusItem: vscode.StatusBarItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 90);
    public static readonly deviceStatusItem: vscode.StatusBarItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 80);

    public static initStatusItemCommands() {
        this.projectStatusItem.command = constants.commandSelectProjectIdentifier;
        this.targetStatusItem.command = constants.commandSelectTargetIdentifier;
        this.deviceStatusItem.command = constants.commandSelectDeviceIdentifier;
    }

    public static updateProjectsStatusItem() {
        this.projectStatusItem.text = Configuration.selectedProject?.name ?? "No project";
        Configuration.workspaceProjects.length === 1 ? this.projectStatusItem.hide() : this.projectStatusItem.show();
    }
    public static updateTargetStatusItem() {
        this.targetStatusItem.text = `${Icon.target} ${Configuration.selectedTarget} | Any CPU`;
        this.targetStatusItem.show();
    }
    public static updateDeviceStatusItem() {
        this.deviceStatusItem.text = `${Icon.device} ${Configuration.selectedDevice?.name}`;
        this.deviceStatusItem.show();
    }


    public static async showQuickPickProject() {
        const items = Configuration.workspaceProjects.map(project => ({
            label: project.name!,
            detail: project.path,
            item: project
        }));
        const selectedItem = (await vscode.window.showQuickPick(items, { placeHolder: "Select active project" }))?.item;
        
        if (selectedItem !== undefined)
            Configuration.selectProject(selectedItem);
    }
    public static async showQuickPickTarget() {
        const items = [ Target.Debug, Target.Release ];
        const selectedItem = ( await vscode.window.showQuickPick(items, { placeHolder: "Select configuration" }));

        if (selectedItem !== undefined)
            Configuration.selectTarget(selectedItem as Target);
    }
    public static async showQuickPickDevice() {
        const items = Configuration.mobileDevices.map(device => ({
            label: `${device.name} • ${device.platform}`,
            detail: `Serial: ${device.serial}`,
            item: device
        }));
        const selectedItem = (await vscode.window.showQuickPick(items, { placeHolder: "Select device" }))?.item;
        
        if (selectedItem !== undefined)
            Configuration.selectDevice(selectedItem);
    }
}

export class Icon {
    public static readonly project = "$(window)"; //todo
    public static readonly target= "$(window)";
    public static readonly device = "$(device-mobile)";
}
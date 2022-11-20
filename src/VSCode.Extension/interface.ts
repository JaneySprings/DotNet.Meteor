import * as vscode from 'vscode';
import { Configuration, Target } from './configuration';
import { Command } from './constants';

export class Interface {
    public static projectStatusItem: vscode.StatusBarItem
    public static targetStatusItem: vscode.StatusBarItem
    public static deviceStatusItem: vscode.StatusBarItem

    public static activate() {
        this.projectStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 100);
        this.targetStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 90);
        this.deviceStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 80);

        this.projectStatusItem.command = Command.selectProject;
        this.targetStatusItem.command = Command.selectTarget;
        this.deviceStatusItem.command = Command.selectDevice;
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
            label: project.name,
            detail: project.path,
            item: project
        }));
        const selectedItem = (await vscode.window.showQuickPick(items, { placeHolder: "Select active project" }))?.item;
        
        if (selectedItem !== undefined)
            Configuration.performSelectProject(selectedItem);
    }
    public static async showQuickPickTarget() {
        const items = [ Target.Debug, Target.Release ];
        const selectedItem = ( await vscode.window.showQuickPick(items, { placeHolder: "Select configuration" }));

        if (selectedItem !== undefined)
            Configuration.performSelectTarget(selectedItem as Target);
    }
    public static async showQuickPickDevice() {
        const items = Configuration.mobileDevices.map(device => ({
            label: `${device.is_running ? "▶︎ " : ""}${device.name}`,
            detail: device.os_version ?? device.platform,
            item: device
        }));
        const selectedItem = (await vscode.window.showQuickPick(items, { placeHolder: "Select device" }))?.item;
        
        if (selectedItem !== undefined)
            Configuration.performSelectDevice(selectedItem);
    }
}

export class Icon {
    public static readonly project = "$(window)"; //todo
    public static readonly target= "$(window)";
    public static readonly device = "$(device-mobile)";
    public static readonly active = "$(play-circle)";
}
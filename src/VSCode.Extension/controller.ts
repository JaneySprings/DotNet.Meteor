import * as vscode from 'vscode';
import { Project, Device, Icon } from "./models"
import { Configuration, Target } from './configuration';
import { DebuggerUtils } from "./bridge";
import { Command } from './constants';


export class ViewController {
    public static projectStatusItem: vscode.StatusBarItem;
    public static targetStatusItem: vscode.StatusBarItem;
    public static deviceStatusItem: vscode.StatusBarItem;
    public static workspaceProjects: Project[];
    public static mobileDevices: Device[];
    public static isDebugging: Boolean;

    
    public static activate() {
        this.projectStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 100);
        this.targetStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 90);
        this.deviceStatusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 80);

        this.projectStatusItem.command = Command.selectProject;
        this.targetStatusItem.command = Command.selectTarget;
        this.deviceStatusItem.command = Command.selectDevice;
    }


    public static performSelectProject(item: Project) {
        Configuration.selectedProject = item;
        this.projectStatusItem.text = `${Icon.project} ${Configuration.selectedProject?.name}`;
        ViewController.workspaceProjects.length === 1 ? this.projectStatusItem.hide() : this.projectStatusItem.show();
    }
    public static performSelectTarget(target: Target) {
        Configuration.selectedTarget = target;
        this.targetStatusItem.text = `${Icon.target} ${Configuration.selectedTarget} | Any CPU`;
        this.targetStatusItem.show();
    }
    public static performSelectDevice(item: Device) {
        Configuration.selectedDevice = item;
        this.deviceStatusItem.text = `${Icon.device} ${Configuration.selectedDevice?.name}`;
        this.deviceStatusItem.show();
    }
    public static performSelectDefaults() {
        ViewController.performSelectProject(ViewController.workspaceProjects[0]);
        ViewController.performSelectDevice(ViewController.mobileDevices[0]);
        ViewController.performSelectTarget(Target.Debug);
    }


    public static fetchWorkspace() {
        const workspacePath = Configuration.geWorkspacePath();
        ViewController.workspaceProjects = DebuggerUtils.findProjects(workspacePath);
    }
    public static fetchDevices() {
        const androidDevices = DebuggerUtils.androidDevices();
        const appleDevices = DebuggerUtils.appleDevices();
        ViewController.mobileDevices = androidDevices.concat(appleDevices);
    }


    public static async showQuickPickProject() {
        const items = ViewController.workspaceProjects.map(project => Project.toDisplayItem(project));
        const selectedItem = await vscode.window.showQuickPick(items, { placeHolder: "Select active project" });

        if (selectedItem !== undefined) {
            ViewController.performSelectProject(selectedItem.item);
        }
    }
    public static async showQuickPickTarget() {
        const items = [ Target.Debug, Target.Release ];
        const selectedItem = await vscode.window.showQuickPick(items, { placeHolder: "Select configuration" });
        
        if (selectedItem !== undefined) {
            ViewController.performSelectTarget(selectedItem as Target);
        }
    }
    public static async showQuickPickDevice() {
        ViewController.fetchDevices();

        const items = ViewController.mobileDevices.map(device => Device.toDisplayItem(device));
        const selectedItem = await vscode.window.showQuickPick(items, { placeHolder: "Select device" });

        if (selectedItem !== undefined) {
            ViewController.performSelectDevice(selectedItem.item);
        }
    }
}
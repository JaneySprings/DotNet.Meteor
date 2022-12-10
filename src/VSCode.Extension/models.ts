import { Configuration } from './configuration';
import * as vscode from 'vscode';


export enum Target {
    Debug = "Debug",
    Release = "Release"
}


export class Project {
    public name!: string;
    public path!: string;
    public frameworks: string[] | undefined;
}

export class ProjectItem implements vscode.QuickPickItem {
    label: string;
    description: string;
    detail: string;
    item: Project;

    constructor(project: Project) {
        const workspace = Configuration.workspacePath();
        this.label = project.name;
        this.description = project.path.replace(workspace + '/', '');
        this.detail = project.frameworks?.join('  ') ?? "frameworks not found";
        this.item = project;
    }
}


export class Device {
    public name: string | undefined;
    public details: string | undefined;
    public serial: string | undefined;
    public platform: string | undefined;
    public os_version: string | undefined;
    public is_emulator: boolean | undefined;
    public is_running: boolean | undefined;
}

export class DeviceItem implements vscode.QuickPickItem {
    label: string;
    detail: string;
    item: Device;

    constructor(device: Device) {
        this.label = `${device.is_running ? Icon.active : Icon.inactive} ${device.name}`;
        this.detail = `${device.details} • ${device.os_version ?? device.platform}`;
        this.item = device;
    }
}


export class Icon {
    public static readonly project = "$(window)";
    public static readonly target= "$(window)";
    public static readonly device = "$(device-mobile)";
    public static readonly active = "$(vm-running)";
    public static readonly inactive = "$(device-mobile)";
}
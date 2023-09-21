import { QuickPickItem, QuickPickItemKind } from 'vscode';


export class Icon {
    public static readonly project = "$(window)";
    public static readonly target= "$(window)";
    public static readonly device = "$(device-mobile)";
    public static readonly computer = "$(vm)";
    public static readonly active = "$(debug-disconnect)";
    public static readonly inactive = "$(device-mobile)";
}

export enum Target {
    Debug = "Debug",
    Release = "Release"
}

export interface IProject {
    name: string;
    path: string;
    frameworks: string[];
}

export interface IDevice {
    name: string | undefined;
    detail: string | undefined;
    serial: string | undefined;
    platform: string | undefined;
    os_version: string | undefined;
    runtime_id: string | undefined;
    is_emulator: boolean | undefined;
    is_running: boolean | undefined;
    is_mobile: boolean | undefined;
}


export class ProjectItem implements QuickPickItem {
    label: string;
    description: string;
    detail: string;
    item: IProject;

    constructor(project: IProject) {
        this.label = project.name;
        this.detail = project.path;
        this.description = project.frameworks?.join('  ') ?? "frameworks not found";
        this.item = project;
    }
}

export class DeviceItem implements QuickPickItem {
    label: string;
    description: string;
    item: IDevice;

    constructor(device: IDevice) {
        this.label = `${device.is_running ? Icon.active : Icon.inactive} ${device.name}`;
        this.description = device.os_version ?? '';
        this.item = device;
    }
}

export class SeparatorItem implements QuickPickItem {
    kind: QuickPickItemKind = QuickPickItemKind.Separator;
    label: string;

    constructor(label: string | undefined) {
        this.label = label ?? '';
    }
}
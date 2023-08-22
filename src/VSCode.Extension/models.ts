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

export class Project {
    public name!: string;
    public path!: string;
    public frameworks!: string[];
}

export class Device {
    public name: string | undefined;
    public detail: string | undefined;
    public serial: string | undefined;
    public platform: string | undefined;
    public os_version: string | undefined;
    public runtime_id: string | undefined;
    public is_emulator: boolean | undefined;
    public is_running: boolean | undefined;
    public is_mobile: boolean | undefined;
}


export class ProjectItem implements QuickPickItem {
    label: string;
    description: string;
    detail: string;
    item: Project;

    constructor(project: Project) {
        this.label = project.name;
        this.detail = project.path;
        this.description = project.frameworks?.join('  ') ?? "frameworks not found";
        this.item = project;
    }
}

export class DeviceItem implements QuickPickItem {
    label: string;
    description: string;
    item: Device;

    constructor(device: Device) {
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
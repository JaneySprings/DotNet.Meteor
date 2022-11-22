import { Configuration } from './configuration';


export class Project {
    public name!: string;
    public path!: string;
    public frameworks: string[] | undefined;

    public static toDisplayItem(project: Project) {
        const workspace = Configuration.geWorkspacePath();
        return ({
            label: project.name,
            description: project.path.replace(workspace + '/', ''),
            detail: project.frameworks?.join('  ') ?? "no frameworks",
            item: project
        });
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

    public static toDisplayItem(device: Device) {
        return ({
            label: `${device.is_running ? Icon.active : Icon.inactive} ${device.name}`,
            detail: `${device.details} • ${device.os_version ?? device.platform}`,
            item: device
        })
    }
}

export class Icon {
    public static readonly project = "$(window)";
    public static readonly target= "$(window)";
    public static readonly device = "$(device-mobile)";
    public static readonly active = "$(vm-running)";
    public static readonly inactive = "$(device-mobile)";
}
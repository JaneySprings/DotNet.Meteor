import { Icon } from "./interface";


export class Project {
    public name!: string;
    public path!: string;
    public frameworks: string[] | undefined;
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
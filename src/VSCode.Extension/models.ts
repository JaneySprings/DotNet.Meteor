
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
    public isEmulator: boolean | undefined;
    public isRunning: boolean | undefined;
}
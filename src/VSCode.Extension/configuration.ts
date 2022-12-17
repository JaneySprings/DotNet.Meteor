import { Project, Device, Target } from "./models"
import { Controller } from "./controller";
import { CommandLine } from "./bridge";
import * as res from './resources';
import * as vscode from 'vscode';


export class Configuration {
    public static androidSdk: string;
    public static selectedProject: Project;
    public static selectedDevice: Device;
    public static selectedTarget: Target;

    public static getDebuggingPort(): number {
        if (Configuration.isAndroid()) 
            return vscode.workspace.getConfiguration(res.configId)
                .get(res.configIdMonoSdbDebuggerPortAndroid) ?? res.configDefaultMonoSdbDebuggerPortAndroid;
        
        if (Configuration.isIOS()) 
            return vscode.workspace.getConfiguration(res.configId)
                .get(res.configIdMonoSdbDebuggerPortApple) ?? res.configDefaultMonoSdbDebuggerPortApple;

        if (Configuration.isMacCatalyst())
            return 0;

        return -1;
    }


    public static isAndroid(): boolean {
        return Configuration.selectedDevice.platform?.includes('android') ?? false;
    }
    public static isIOS(): boolean {
        return Configuration.selectedDevice.platform?.includes('ios') ?? false;
    }
    public static isMacCatalyst(): boolean {
        return Configuration.selectedDevice.platform?.includes('maccatalyst') ?? false;
    }


    public static updateAndroidSdk() {
        Configuration.androidSdk = CommandLine.androidSdk();
    }
    public static updateSelectedProject() {
        const project = CommandLine.analyzeProject(Configuration.selectedProject!.path);
        Configuration.selectedProject = project;
    }

    public static workspacePath() {
        return vscode.workspace.workspaceFolders![0].uri.fsPath;
    }

    public static targetFramework(): string | undefined {
        const devicePlatform = Configuration.selectedDevice!.platform;
        return Configuration.selectedProject!.frameworks?.find(it => it.includes(devicePlatform!));
    }

    
    public static validate(): boolean {
        if (!Configuration.selectedProject || !Configuration.selectedProject.path) {
            vscode.window.showErrorMessage(res.messageNoProjectFound);
            return false;
        }
        if (!Configuration.selectedDevice || !Configuration.selectedDevice.platform) {
            vscode.window.showErrorMessage(res.messageNoDeviceFound);
            return false;
        }
        if (!Controller.mobileDevices.some(it => it.name === Configuration.selectedDevice!.name)) {
            vscode.window.showErrorMessage(res.messageDeviceNotExists);
            return false;
        }

        return true;
    }
} 
import { Project, Device } from "./models"
import { DebuggerUtils } from "./bridge";
import * as vscode from 'vscode';
import { ViewController } from "./controller";


export enum Target {
    Debug = "Debug",
    Release = "Release"
}

export class Configuration {
    public static debuggingPort: number;
    public static selectedProject: Project | undefined;
    public static selectedDevice: Device | undefined;
    public static selectedTarget: Target | undefined;

    public static updateDebuggingPort() {
        Configuration.debuggingPort = DebuggerUtils.freePort();
    }

    public static updateSelectedProject() {
        const project = DebuggerUtils.analyzeProject(Configuration.selectedProject!.path);
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
            vscode.window.showErrorMessage('Selected project not found');
            return false;
        }
        if (!Configuration.selectedDevice || !Configuration.selectedDevice.platform) {
            vscode.window.showErrorMessage('Selected device incorrect');
            return false;
        }

        if (!ViewController.mobileDevices.some(it => it.name === Configuration.selectedDevice!.name)) {
            vscode.window.showErrorMessage('Selected device does not exists yet');
            return false;
        }

        return true;
    }
} 
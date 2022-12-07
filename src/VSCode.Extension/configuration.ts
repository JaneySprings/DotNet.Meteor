import { Project, Device, Target } from "./models"
import { CommandLine } from "./bridge";
import * as vscode from 'vscode';
import { Controller } from "./controller";


export class Configuration {
    public static debuggingPort: number;
    public static selectedProject: Project;
    public static selectedDevice: Device;
    public static selectedTarget: Target;

    public static updateDebuggingPort() {
        Configuration.debuggingPort = CommandLine.freePort();
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
            vscode.window.showErrorMessage('Selected project not found');
            return false;
        }
        if (!Configuration.selectedDevice || !Configuration.selectedDevice.platform) {
            vscode.window.showErrorMessage('Selected device incorrect');
            return false;
        }

        if (!Controller.mobileDevices.some(it => it.name === Configuration.selectedDevice!.name)) {
            vscode.window.showErrorMessage('Selected device does not exists yet');
            return false;
        }

        return true;
    }
} 
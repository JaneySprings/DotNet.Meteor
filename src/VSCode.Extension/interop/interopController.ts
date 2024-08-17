import { ProcessArgumentBuilder } from './processArgumentBuilder';
import { ProcessRunner } from './processRunner';
import { Project } from '../models/project';
import { Device } from '../models/device';
import * as vscode from 'vscode';
import * as path from 'path';


export class InteropController {
    private static workspaceToolPath: string;

    public static activate(context: vscode.ExtensionContext) {
        const executableExtension = process.platform === 'win32' ? '.exe' : '';
        InteropController.workspaceToolPath = path.join(context.extensionPath, "extension", "bin", "Workspace", "DotNet.Meteor.Workspace" + executableExtension);
    }

    public static androidSdk(): string | undefined {
        return ProcessRunner.runSync(InteropController.workspaceToolPath, "--android-sdk-path");
    }
    public static async getDevices(): Promise<Device[]> {
        return await ProcessRunner.runAsync<Device[]>(new ProcessArgumentBuilder(InteropController.workspaceToolPath)
            .append("--all-devices"));
    }
    public static async getProjects(folders: string[]): Promise<Project[]> {
        return await ProcessRunner.runAsync<Project[]>(new ProcessArgumentBuilder(InteropController.workspaceToolPath)
            .append("--analyze-workspace")
            .appendQuoted(...folders));
    }
}

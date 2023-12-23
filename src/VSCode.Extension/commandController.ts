import { ProcessArgumentBuilder } from './processes/processArgumentBuilder';
import { ProcessRunner } from './processes/processRunner';
import { Project } from './models/project';
import { Device } from './models/device';
import * as res from './resources/constants';
import * as vscode from 'vscode';
import * as path from 'path';


export class CommandController {
    private static workspaceToolPath: string;
    private static reloadToolPath: string;

    public static activate(context: vscode.ExtensionContext) {
        const extensionPath = vscode.extensions.getExtension(`${res.extensionPublisher}.${res.extensionId}`)?.extensionPath ?? '';
        const executableExtension = process.platform === 'win32' ? '.exe' : '';
        CommandController.workspaceToolPath = path.join(extensionPath, "extension", "bin", "Workspace", "DotNet.Meteor.Workspace" + executableExtension);
        CommandController.reloadToolPath = path.join(extensionPath, "extension", "bin", "HotReload", "DotNet.Meteor.HotReload" + executableExtension);
    }

    public static androidSdk(): string | undefined {
        return ProcessRunner.runSync(CommandController.workspaceToolPath, "--android-sdk-path");
    }
    public static async getDevices(): Promise<Device[]> {
        return await ProcessRunner.runAsync<Device[]>(new ProcessArgumentBuilder(CommandController.workspaceToolPath)
            .append("--all-devices"));
    }
    public static async getProjects(folders: string[]): Promise<Project[]> {
        return await ProcessRunner.runAsync<Project[]>(new ProcessArgumentBuilder(CommandController.workspaceToolPath)
            .append("--analyze-workspace")
            .appendQuoted(...folders));
    }
    public static async xamlReload(port: number, path: string): Promise<boolean>  {
        return await ProcessRunner.runAsync<boolean>(new ProcessArgumentBuilder(CommandController.reloadToolPath)
            .append(port.toString())
            .appendQuoted(path));
    }
}

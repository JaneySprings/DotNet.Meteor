import { ProcessArgumentBuilder } from '../interop/processArgumentBuilder';
import { ProcessRunner } from '../interop/processRunner';
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

    public static async getDevices(): Promise<Device[]> {
        return await ProcessRunner.runAsync<Device[]>(new ProcessArgumentBuilder(InteropController.workspaceToolPath)
            .append("--all-devices"));
    }
    public static async getProjects(folders: string[]): Promise<Project[]> {
        return await ProcessRunner.runAsync<Project[]>(new ProcessArgumentBuilder(InteropController.workspaceToolPath)
            .append("--analyze-workspace")
            .append(...folders));
    }
    public static getAndroidSdk(): string | undefined {
        return ProcessRunner.runSync(new ProcessArgumentBuilder(InteropController.workspaceToolPath)
            .append("--android-sdk-path"));
    }
    public static getPropertyValue(propertyName: string, project: Project, configuration: string, device: Device) : string | undefined {
        const targetFramework = project.frameworks.find(it => it.includes(device.platform ?? 'undefined'));
        const runtimeIdentifier = device?.runtime_id;

        return ProcessRunner.runSync(new ProcessArgumentBuilder("dotnet")
            .append("msbuild").append(project.path)
            .append(`-getProperty:${propertyName}`)
            .conditional(`-p:Configuration=${configuration}`, () => configuration)
            .conditional(`-p:TargetFramework=${targetFramework}`, () => targetFramework)
            .conditional(`-p:RuntimeIdentifier=${runtimeIdentifier}`, () => runtimeIdentifier));
    }
}

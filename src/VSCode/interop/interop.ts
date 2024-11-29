import { ConfigurationController } from '../controllers/configurationController';
import { ProcessArgumentBuilder } from './processArgumentBuilder';
import { ProcessRunner } from './processRunner';
import { Project } from '../models/project';
import { Device } from '../models/device';
import * as path from 'path';


export class Interop {
    private static workspaceToolPath: string;

    public static initialize(extensionPath : string) {
        const executableExtension = ConfigurationController.onWindows ? '.exe' : '';
        Interop.workspaceToolPath = path.join(extensionPath, "extension", "bin", "Workspace", "DotNet.Meteor.Workspace" + executableExtension);
        Interop.init();
    }

    private static init() {
        // This call is hanging because the child processes is not exiting
        ProcessRunner.runAsync<boolean>(new ProcessArgumentBuilder(Interop.workspaceToolPath)
            .append("--initialize"));
    }

    public static async getDevices(): Promise<Device[]> {
        return await ProcessRunner.runAsync<Device[]>(new ProcessArgumentBuilder(Interop.workspaceToolPath)
            .append("--all-devices"));
    }
    public static async getProjects(folders: string[]): Promise<Project[]> {
        return await ProcessRunner.runAsync<Project[]>(new ProcessArgumentBuilder(Interop.workspaceToolPath)
            .append("--analyze-workspace")
            .append(...folders));
    }
    public static getAndroidSdk(): string | undefined {
        return ProcessRunner.runSync(new ProcessArgumentBuilder(Interop.workspaceToolPath)
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

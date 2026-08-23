import { ConfigurationController } from '../controllers/configurationController';
import { ProcessArgumentBuilder } from './processArgumentBuilder';
import { ProcessRunner } from './processRunner';
import { Project } from '../models/project';
import { Device } from '../models/device';
import * as path from 'path';


export class Interop {
    public static workspaceToolPath: string;
    public static customTargetsPath: string;

    public static initialize(extensionPath: string): boolean {
        Interop.workspaceToolPath = path.join(extensionPath, "extension", "bin", "Workspace", "meteor.dll");
        Interop.customTargetsPath = path.join(extensionPath, "extension", "bin", "Workspace", "CopyRemoteCoreclrTargetLibrary.targets");

        if (Interop.getMeteorVersion() === undefined)
            return false;

        Interop.init();
        return true;
    }

    private static init() {
        // This call is hanging because the child processes is not exiting
        ProcessRunner.runAsync<boolean>(new ProcessArgumentBuilder("dotnet")
            .append(Interop.workspaceToolPath)
            .append("--initialize"));
    }

    public static async getDevices(): Promise<Device[]> {
        return await ProcessRunner.runAsync<Device[]>(new ProcessArgumentBuilder("dotnet")
            .append(Interop.workspaceToolPath)
            .append("--all-devices"));
    }
    public static getMeteorVersion(): string | undefined {
        return ProcessRunner.runSync(new ProcessArgumentBuilder("dotnet")
            .append(Interop.workspaceToolPath)
            .append("--version"));
    }
    public static getAndroidSdk(): string | undefined {
        return ProcessRunner.runSync(new ProcessArgumentBuilder("dotnet")
            .append(Interop.workspaceToolPath)
            .append("--android-sdk-path"));
    }
    public static getPropertyValue(propertyName: string, project: Project, configuration: string, device: Device): string | undefined {
        const targetFramework = ConfigurationController.targetFramework;
        const runtimeIdentifier = device?.runtime_id;

        return ProcessRunner.runSync(new ProcessArgumentBuilder("dotnet")
            .append("msbuild").append(project.path)
            .append(`-getProperty:${propertyName}`)
            .conditional(`-p:Configuration=${configuration}`, () => configuration)
            .conditional(`-p:TargetFramework=${targetFramework}`, () => targetFramework)
            .conditional(`-p:RuntimeIdentifier=${runtimeIdentifier}`, () => runtimeIdentifier));
    }
}

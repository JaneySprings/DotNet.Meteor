import path = require('path');
import { ProcessRunner, ProcessArgumentBuilder } from './executor';
import { Project, Device } from './models';
import { Configuration } from './configuration';
import * as res from './resources';
import * as vscode from 'vscode';

export class CommandLine {
    private static toolPath: string = path.join(
        vscode.extensions.getExtension(`${res.extensionPublisher}.${res.extensionId}`)?.extensionPath ?? "?",
        "extension", "bin", "dotnet-mobile.dll");

    public static mobileDevicesAsync(callback: (items: Device[]) => any) {
        ProcessRunner.runAsync<Device[]>(new ProcessArgumentBuilder("dotnet")
            .append(this.toolPath)
            .append("--all-devices"), callback);
    }

    public static deviceInfo(device: Device): Device {
        return ProcessRunner.run<Device>(new ProcessArgumentBuilder("dotnet")
            .append(this.toolPath)
            .append("--device")
            .append(`"${device.platform}"`)
            .append(`"${device.name}"`));
    }

    public static analyzeWorkspaceAsync(callback: (items: Project[]) => any) {
        ProcessRunner.runAsync<Project[]>(new ProcessArgumentBuilder("dotnet")
            .append(this.toolPath)
            .append("--analyze-workspace")
            .append(`"${Configuration.workspacePath()}"`), callback);
    }

    public static analyzeProject(projectFile: string): Project {
        return ProcessRunner.run<Project>(new ProcessArgumentBuilder("dotnet")
            .append(this.toolPath)
            .append("--analyze-project")
            .append(`"${projectFile}"`));
    }

    public static androidSdk(): string {
        return ProcessRunner.run<string>(new ProcessArgumentBuilder("dotnet")
            .append(this.toolPath)
            .append("--android-sdk-path"));
    }

    public static freePort(): number {
        return ProcessRunner.run<number>(new ProcessArgumentBuilder("dotnet")
            .append(this.toolPath)
            .append("--free-port"));
    }
}
  
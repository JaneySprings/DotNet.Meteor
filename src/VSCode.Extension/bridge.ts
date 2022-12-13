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
            .appendQuoted(this.toolPath)
            .append("--all-devices"), callback);
    }

    public static deviceInfo(device: Device): Device {
        return ProcessRunner.run<Device>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(this.toolPath)
            .append("--device")
            .appendQuoted(device.platform ?? "")
            .appendQuoted(device.name ?? ""));
    }

    public static analyzeWorkspaceAsync(callback: (items: Project[]) => any) {
        ProcessRunner.runAsync<Project[]>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(this.toolPath)
            .append("--analyze-workspace")
            .appendQuoted(Configuration.workspacePath()), callback);
    }

    public static analyzeProject(projectFile: string): Project {
        return ProcessRunner.run<Project>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(this.toolPath)
            .append("--analyze-project")
            .appendQuoted(projectFile));
    }

    public static androidSdk(): string {
        return ProcessRunner.run<string>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(this.toolPath)
            .append("--android-sdk-path"));
    }
}
  
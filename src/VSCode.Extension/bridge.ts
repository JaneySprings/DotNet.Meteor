import path = require('path');
import { ProcessRunner, ProcessArgumentBuilder } from './executor';
import { Configuration } from './configuration';
import { Project, Device } from './models';
import { extensions } from 'vscode';
import * as res from './resources';

export class CommandLine {
    private static toolPath: string = path.join(
        extensions.getExtension(`${res.extensionPublisher}.${res.extensionId}`)?.extensionPath ?? "?",
        "extension", "bin", "dotnet-mobile.dll");

    public static mobileDevicesAsync(callback: (items: Device[]) => any) {
        ProcessRunner.runAsync<Device[]>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(this.toolPath)
            .append("--all-devices"), callback);
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
  
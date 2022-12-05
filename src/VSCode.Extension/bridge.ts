import path = require('path');
import { ProcessRunner, ProcessArgumentBuilder } from './executor';
import { Project, Device } from './models';
import { extensionPath } from './constants';
import { Configuration } from './configuration';

export class CommandLine {
    private static toolPath: string = path.join(extensionPath, "extension", "bin", "dotnet-mobile.dll");

    public static mobileDevicesAsync(callback: (items: Device[]) => any) {
        ProcessRunner.runAsync<Device[]>(new ProcessArgumentBuilder("dotnet")
            .append(this.toolPath)
            .append("--all-devices"), callback);
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

    public static runEmulator(name: string): string {
        return ProcessRunner.run<string>(new ProcessArgumentBuilder("dotnet")
            .append(this.toolPath)
            .append("--run-emulator")
            .append(name));
    }

    public static freePort(): number {
        return ProcessRunner.run<number>(new ProcessArgumentBuilder("dotnet")
            .append(this.toolPath)
            .append("--free-port"));
    }
}
  
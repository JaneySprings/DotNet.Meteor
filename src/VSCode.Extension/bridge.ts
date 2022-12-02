import path = require('path');
import { ProcessRunner, ProcessArgumentBuilder } from './executor';
import { Project, Device } from './models';
import { extensionInstance } from './constants';

export class DebuggerUtils {
    private static toolPath: string = path.join(extensionInstance!.extensionPath, "extension", "bin", "dotnet-mobile.dll");

    public static androidDevices(): Device[] {
        return ProcessRunner.run<Device[]>(new ProcessArgumentBuilder("dotnet")
            .append(this.toolPath)
            .append("--android-devices"));
    }

    public static appleDevices(): Device[] {
        return ProcessRunner.run<Device[]>(new ProcessArgumentBuilder("dotnet")
            .append(this.toolPath)
            .append("--apple-devices"));
    }

    public static findProjects(workspaceRoot: string): Project[] {
        return ProcessRunner.run<Project[]>(new ProcessArgumentBuilder("dotnet")
            .append(this.toolPath)
            .append("--find-projects")
            .append(`"${workspaceRoot}"`));
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
  
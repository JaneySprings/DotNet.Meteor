import { execSync, exec } from 'child_process';
import { Project, Device } from './models';
import * as res from './resources';
import * as vscode from 'vscode';
import * as path from 'path';


export class CommandController {
    private static toolPath: string;

    public static activate(context: vscode.ExtensionContext): boolean {
        const qualifiedVersion = CommandController.runtimeVersion();
        const qualifiedVersionRegex = new RegExp('^\\d+\\.\\d+', ''); 
        const versionRegexCollection = qualifiedVersionRegex.exec(qualifiedVersion);
        if (!versionRegexCollection || versionRegexCollection.length === 0) {
            vscode.window.showErrorMessage(res.messageRuntimeNotFound);
            return false;
        }

        const version = versionRegexCollection[0];
        const extensionPath = vscode.extensions.getExtension(`${res.extensionPublisher}.${res.extensionId}`)?.extensionPath ?? '';
        CommandController.toolPath = path.join(extensionPath, "extension", "bin", `net${version}`, "DotNet.Meteor.Workspace.dll");
        return true;
    }

    public static androidSdk(): string {
        return ProcessRunner.runSync<string>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(CommandController.toolPath)
            .append("--android-sdk-path"));
    }
    public static async getDevices(): Promise<Device[]> {
        return await ProcessRunner.runAsync<Device[]>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(CommandController.toolPath)
            .append("--all-devices"));
    }
    public static async getProjects(folders: string[]): Promise<Project[]> {
        return await ProcessRunner.runAsync<Project[]>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(CommandController.toolPath)
            .append("--analyze-workspace")
            .appendQuoted(...folders));
    }
    public static async xamlSchema(path: string): Promise<boolean>  {
        return await ProcessRunner.runAsync<boolean>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(CommandController.toolPath)
            .append("--xaml")
            .appendQuoted(path));
    }
    public static async xamlReload(port: number, path: string): Promise<boolean>  {
        return await ProcessRunner.runAsync<boolean>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(CommandController.toolPath)
            .append("--xaml-reload")
            .append(port.toString())
            .appendQuoted(path));
    }
    public static runtimeVersion(): string {
        return ProcessRunner.execSync(new ProcessArgumentBuilder("dotnet")
            .append("--version"));
    }
}

class ProcessRunner {
    public static execSync(builder: ProcessArgumentBuilder): string {
        return execSync(builder.build()).toString()
    }
    public static runSync<TModel>(builder: ProcessArgumentBuilder): TModel {
        const result = ProcessRunner.execSync(builder);
        return JSON.parse(result);
    }
    public static async runAsync<TModel>(builder: ProcessArgumentBuilder): Promise<TModel> {
        return new Promise<TModel>((resolve, reject) => {
            exec(builder.build(), (error, stdout, stderr) => {
                if (error) {
                    vscode.window.showErrorMessage(`${res.extensionId}: ${stderr}`);
                    reject(stderr);
                }

                resolve(JSON.parse(stdout.toString()));
            })
        });
    }
}

export class ProcessArgumentBuilder {
    private arguments: string[] = [];

    public constructor(command: string) {
        this.arguments.push(command);
    }

    public append(arg: string): ProcessArgumentBuilder {
        this.arguments.push(arg);
        return this;
    }
    public appendQuoted(...args: string[]): ProcessArgumentBuilder {
        args.forEach(a => this.arguments.push(`"${a}"`));
        return this;
    }
    public override(arg: string): ProcessArgumentBuilder {
        const argName = arg.split("=")[0];
        const index = this.arguments.findIndex(a => a.startsWith(argName));
        if (index > -1) 
            this.arguments.splice(index, 1);
        this.arguments.push(arg);
        return this;
    }
    public build(): string {
        return this.arguments.join(" ");
    }
}
  
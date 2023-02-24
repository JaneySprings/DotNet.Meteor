import path = require('path');
import { execSync, exec } from 'child_process';
import { Project, Device } from './models';
import { extensions } from 'vscode';
import * as res from './resources';


export class CommandInterface {
    private static extensionPath: string = extensions.getExtension(`${res.extensionPublisher}.${res.extensionId}`)?.extensionPath ?? '';
    private static toolPath: string = path.join(CommandInterface.extensionPath, "extension", "bin", "DotNet.Meteor.Debug.dll"); 

    public static devicesAsync(callback: (items: Device[]) => any) {
        ProcessRunner.run<Device[]>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(CommandInterface.toolPath)
            .append("--all-devices"), callback);
    }
    public static analyzeWorkspaceAsync(folders: string[], callback: (items: Project[]) => any) {
        ProcessRunner.run<Project[]>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(CommandInterface.toolPath)
            .append("--analyze-workspace")
            .appendRangeQuoted(folders), callback);
    }
    public static androidSdk(): string {
        return ProcessRunner.runSync<string>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(CommandInterface.toolPath)
            .append("--android-sdk-path"));
    }
    public static async xamlSchema(path: string): Promise<boolean>  {
        return await ProcessRunner.runAsync<boolean>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(CommandInterface.toolPath)
            .append("--xaml")
            .appendQuoted(path));
    }
}

class ProcessRunner {
    public static runSync<TModel>(builder: ProcessArgumentBuilder): TModel {
        const result = execSync(builder.build()).toString();
        return JSON.parse(result);
    }
    public static async runAsync<TModel>(builder: ProcessArgumentBuilder): Promise<TModel> {
        return new Promise<TModel>((resolve, reject) => {
            exec(builder.build(), (error, stdout, stderr) => {
                if (error) {
                    console.error(error);
                    reject(error);
                } else {
                    const item: TModel = JSON.parse(stdout.toString());
                    resolve(item);
                }
            })
        });
    }
    public static run<TModel>(builder: ProcessArgumentBuilder, callback: (model: TModel) => any) {
        exec(builder.build(), (error, stdout, stderr) => {
            if (error) {
                console.error(error);
            } else {
                const item: TModel = JSON.parse(stdout.toString());
                callback(item);
            }
        })
    }
}

export class ProcessArgumentBuilder {
    private args: string[] = [];

    public constructor(command: string) {
        this.args.push(command);
    }

    public append(arg: string): ProcessArgumentBuilder {
        this.args.push(arg);
        return this;
    }
    public appendQuoted(arg: string): ProcessArgumentBuilder {
        this.args.push(`"${arg}"`);
        return this;
    }
    public appendRangeQuoted(arg: string[]): ProcessArgumentBuilder {
        arg.forEach(a => this.args.push(`"${a}"`));
        return this;
    }
    public override(arg: string): ProcessArgumentBuilder {
        const argName = arg.split("=")[0];
        const index = this.args.findIndex(a => a.startsWith(argName));
        if (index > -1) 
            this.args.splice(index, 1);
        this.args.push(arg);
        return this;
    }
    public build(): string {
        return this.args.join(" ");
    }
}
  
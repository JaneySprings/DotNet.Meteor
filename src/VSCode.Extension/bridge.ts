import path = require('path');
import { execSync, exec } from 'child_process';
import { Project, Device } from './models';
import { extensions } from 'vscode';
import * as res from './resources';


export class CommandInterface {
    private static extensionPath: string = extensions.getExtension(`${res.extensionPublisher}.${res.extensionId}`)?.extensionPath ?? '';
    private static toolPath: string = path.join(CommandInterface.extensionPath, "extension", "bin", "DotNet.Meteor.Debug.dll");
    public static generatedPath: string = path.join(CommandInterface.extensionPath, "extension", "generated");
    

    public static devicesAsync(callback: (items: Device[]) => any) {
        ProcessRunner.runAsync<Device[]>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(CommandInterface.toolPath)
            .append("--all-devices"), callback);
    }
    public static analyzeWorkspaceAsync(folders: string[], callback: (items: Project[]) => any) {
        ProcessRunner.runAsync<Project[]>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(CommandInterface.toolPath)
            .append("--analyze-workspace")
            .appendRangeQuoted(folders), callback);
    }
    public static androidSdk(): string {
        return ProcessRunner.run<string>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(CommandInterface.toolPath)
            .append("--android-sdk-path"));
    }
    public static xamlSchema(path: string, framework: string, rid: string, callback: (succeeded: boolean) => any)  {
        ProcessRunner.runAsync<boolean>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(CommandInterface.toolPath)
            .append("--xaml")
            .appendQuoted(path)
            .appendQuoted(CommandInterface.generatedPath)
            .append(framework)
            .append(rid), callback);
    }
}

class ProcessRunner {
    public static run<TModel>(builder: ProcessArgumentBuilder): TModel {
        const result = execSync(builder.build()).toString();
        return JSON.parse(result);
    }
    public static runAsync<TModel>(builder: ProcessArgumentBuilder, callback: (model: TModel) => any) {
        exec(builder.build(), (error, stdout, stderr) => {
            if (error) {
                console.error(error);
                process.exit(1);
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
  
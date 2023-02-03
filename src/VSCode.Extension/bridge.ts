import path = require('path');
import { execSync, exec } from 'child_process';
import { Configuration } from './configuration';
import { Project, Device } from './models';
import { extensions } from 'vscode';
import * as res from './resources';


export class CommandInterface {
    private static toolPath: string = path.join(
        extensions.getExtension(`${res.extensionPublisher}.${res.extensionId}`)?.extensionPath ?? "?",
        "extension", "bin", "DotNet.Meteor.Debug.dll");

    public static mobileDevicesAsync(callback: (items: Device[]) => any) {
        ProcessRunner.runAsync<Device[]>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(this.toolPath)
            .append("--all-devices"), callback);
    }
    public static analyzeWorkspaceAsync(callback: (items: Project[]) => any) {
        ProcessRunner.runAsync<Project[]>(new ProcessArgumentBuilder("dotnet")
            .appendQuoted(this.toolPath)
            .append("--analyze-workspace")
            .appendRangeQuoted(Configuration.workspacesPath()), callback);
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
  
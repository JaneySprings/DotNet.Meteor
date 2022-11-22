import * as vscode from 'vscode';
import { taskProviderType } from './constants';
import { Configuration } from './configuration';
import { DebuggerUtils } from "./bridge";
import { ViewController } from './controller';


export class DotNetTaskProvider implements vscode.TaskProvider {
    provideTasks(token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task[]> {
        return [ DotNetTask.build() ];
    }
    resolveTask(task: vscode.Task, token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task> {
        return DotNetTask.build();
    }
}

class DotNetTaskDefinition implements vscode.TaskDefinition {
    public name: string = "dotnet-meteor build";
    public type: string = taskProviderType;
}

class DotNetTask {
    public static readonly title: string = "Build";
    public static readonly source: string = "dotnet-meteor";

    private static readonly empty: vscode.Task = new vscode.Task(
        new DotNetTaskDefinition(), vscode.TaskScope.Workspace, DotNetTask.title, DotNetTask.source
    );

    public static build(): vscode.Task {
        if (!Configuration.validate())
            return DotNetTask.empty;
    
        const devicePlatform = Configuration.selectedDevice!.platform;
        const framework = Configuration.selectedProject!.frameworks?.find(it => it.includes(devicePlatform!));
        
        if (!framework) {
            vscode.window.showErrorMessage(`No framework for '${devicePlatform}' found`);
            return DotNetTask.empty;
        }

        const command = [
            `dotnet build ${Configuration.selectedProject!.path}`,
            `-c:${Configuration.selectedTarget!}`,
            `-f:${framework}`
        ];
        
        if (Configuration.selectedDevice!.platform?.includes('android') && ViewController.isDebugging) {
            if (!Configuration.selectedDevice!.is_running) {
                const serial = DebuggerUtils.runEmulator(Configuration.selectedDevice!.name!);

                Configuration.selectedDevice!.serial = serial;
                Configuration.selectedDevice!.is_running = true;
            }

            command.push(`-p:AndroidAttachDebugger=true`);
            command.push(`-p:AdbTarget=-s%20${Configuration.selectedDevice!.serial}`);
            command.push(`-p:AndroidSdbTargetPort=${Configuration.debuggingPort}`);
			command.push(`-p:AndroidSdbHostPort=${Configuration.debuggingPort}`);
            command.push('-t:Run');
        }

        return new vscode.Task(
            new DotNetTaskDefinition(), 
            vscode.TaskScope.Workspace, 
            DotNetTask.title, 
            DotNetTask.source, 
            new vscode.ShellExecution(command.join(' '))
        );
    }
}
import * as vscode from 'vscode';
import { Configuration } from './configuration';
import { CommandLine } from "./bridge";
import { Controller } from './controller';


class DotNetTaskDefinition implements vscode.TaskDefinition {
    public name: string = DotNetBuildTaskProvider.action;
    public type: string = DotNetBuildTaskProvider.type;
}

export class DotNetBuildTaskProvider implements vscode.TaskProvider {
    public static source: string = 'dotnet-meteor';
    public static action: string = 'build';
    public static type: string = `${DotNetBuildTaskProvider.source}.${DotNetBuildTaskProvider.action}`

    resolveTask(task: vscode.Task, token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task> { return task; }
    provideTasks(token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task[]> {
        Configuration.updateSelectedProject();

        if (!Configuration.validate())
            return [];
    
        const framework = Configuration.targetFramework();
        const command = [
            `dotnet build "${Configuration.selectedProject!.path}"`,
            `-c:${Configuration.selectedTarget!}`,
            `-f:${framework}`
        ];
        
        if (!framework) {
            vscode.window.showErrorMessage(`No supported framework found`);
            return [];
        }

        if (Controller.isDebugging) {
            if (Configuration.selectedDevice!.platform?.includes('android')) {
                if (!Configuration.selectedDevice!.is_running) {
                    const serial = CommandLine.runEmulator(Configuration.selectedDevice!.name!);
    
                    Configuration.selectedDevice!.serial = serial;
                    Configuration.selectedDevice!.is_running = true;
                }
    
                command.push(`-p:AndroidAttachDebugger=true`);
                command.push(`-p:AdbTarget=-s%20${Configuration.selectedDevice!.serial}`);
                command.push(`-p:AndroidSdbTargetPort=${Configuration.debuggingPort}`);
                command.push(`-p:AndroidSdbHostPort=${Configuration.debuggingPort}`);
                command.push('-t:Run');
            }
    
            if (Configuration.selectedDevice!.platform?.includes('ios')) {
                if (!Configuration.selectedDevice!.is_emulator) {
                    command.push(`-p:RuntimeIdentifier=ios-arm64`);
                }
            }
        }
        
        return [ 
            new vscode.Task(
                new DotNetTaskDefinition(), 
                vscode.TaskScope.Workspace, 
                DotNetBuildTaskProvider.action, 
                DotNetBuildTaskProvider.source,
                new vscode.ShellExecution(command.join(' '))
            )
        ];
    }
}
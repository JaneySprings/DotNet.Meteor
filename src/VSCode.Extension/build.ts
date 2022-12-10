import * as vscode from 'vscode';
import { Configuration } from './configuration';


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
        Configuration.updateAndroidSdk();

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

        if (Configuration.selectedDevice!.platform?.includes('android')) {
            command.push(`-p:AndroidSdkDirectory="${Configuration.androidSdk}"`);
        }

        if (Configuration.selectedDevice!.platform?.includes('ios')) {
            if (!Configuration.selectedDevice!.is_emulator) {
                command.push(`-p:RuntimeIdentifier=ios-arm64`);
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
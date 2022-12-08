import * as vscode from 'vscode';
import { Configuration } from './configuration';


class DotNetTaskDefinition implements vscode.TaskDefinition {
    public name: string = DotNetPublishTaskProvider.action;
    public type: string = DotNetPublishTaskProvider.type;
}

export class DotNetPublishTaskProvider implements vscode.TaskProvider {
    public static source: string = 'dotnet-meteor';
    public static action: string = 'publish';
    public static type: string = `${DotNetPublishTaskProvider.source}.${DotNetPublishTaskProvider.action}`

    resolveTask(task: vscode.Task, token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task> { return task; }
    provideTasks(token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task[]> {
        Configuration.updateSelectedProject();
        Configuration.updateAndroidSdk();

        if (!Configuration.validate())
            return [];
    
        const framework = Configuration.targetFramework();
        const command = [
            `dotnet publish "${Configuration.selectedProject!.path}"`,
            `-c:${Configuration.selectedTarget!}`,
            `-f:${framework}`
        ];
        
        if (!framework) {
            vscode.window.showErrorMessage(`No supported framework found`);
            return [];
        }

        if (Configuration.selectedDevice!.platform?.includes('android')) {
            command.push(`-p:AndroidSdkDirectory=${Configuration.androidSdk}`);
        }

        if (Configuration.selectedDevice!.platform?.includes('ios')) {
            command.push(`-p:RuntimeIdentifier=ios-arm64`);
        }
        
        return [ 
            new vscode.Task(
                new DotNetTaskDefinition(), 
                vscode.TaskScope.Workspace, 
                DotNetPublishTaskProvider.action, 
                DotNetPublishTaskProvider.source,
                new vscode.ShellExecution(command.join(' '))
            )
        ];
    }
}
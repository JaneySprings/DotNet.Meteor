import { ProcessArgumentBuilder } from '../executor';
import { Configuration } from '../configuration';
import * as res from '../resources';
import * as vscode from 'vscode';


class DotNetTaskDefinition implements vscode.TaskDefinition {
    public name: string = res.taskActionPublish;
    public type: string = res.taskIdPublish;
}

export class DotNetPublishTaskProvider implements vscode.TaskProvider {
    resolveTask(task: vscode.Task, token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task> { return task; }
    provideTasks(token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task[]> {
        Configuration.updateSelectedProject();
        if (!Configuration.validate())
            return [];
    
        const framework = Configuration.targetFramework();
        const builder = new ProcessArgumentBuilder('dotnet')
            .append('publish')
            .appendQuoted(Configuration.selectedProject.path)
            .append(`-c:${Configuration.selectedTarget}`)
            .append(`-f:${framework}`)
        
        if (!framework) {
            vscode.window.showErrorMessage(res.messageNoFrameworkFound);
            return [];
        }

        if (Configuration.selectedDevice.runtime_id && Configuration.selectedDevice.runtime_id !== 'maccatalyst-x64') {
            builder.append(`-p:RuntimeIdentifier=${Configuration.selectedDevice.runtime_id}`);
        }
        if (Configuration.isAndroid()) {
            builder.append(`-p:AndroidSdkDirectory="${Configuration.getAndroidSdkDirectory()}"`);
        }
        
        return [ 
            new vscode.Task(
                new DotNetTaskDefinition(), 
                vscode.TaskScope.Workspace, 
                res.taskActionPublish, 
                res.extensionId,
                new vscode.ShellExecution(builder.build())
            )
        ];
    }
}
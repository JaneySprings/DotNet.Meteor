import { ProcessArgumentBuilder } from '../executor';
import { Configuration } from '../configuration';
import * as res from '../resources';
import * as vscode from 'vscode';


class DotNetTaskDefinition implements vscode.TaskDefinition {
    public name: string = res.taskActionBuild;
    public type: string = res.taskIdBuild;
}

export class DotNetBuildTaskProvider implements vscode.TaskProvider {
    resolveTask(task: vscode.Task, token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task> { return task; }
    provideTasks(token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task[]> {
        Configuration.updateSelectedProject();
        if (!Configuration.validate())
            return [];
    
        const framework = Configuration.targetFramework();
        const builder = new ProcessArgumentBuilder('dotnet')
            .append('build')
            .appendQuoted(Configuration.selectedProject.path)
            .append(`-c:${Configuration.selectedTarget}`)
            .append(`-f:${framework}`)
            .append(`-t:Build`);
        
        if (!framework) {
            vscode.window.showErrorMessage(res.messageNoFrameworkFound);
            return [];
        }

        if (Configuration.selectedDevice.runtime_id && Configuration.selectedDevice.runtime_id !== 'maccatalyst-x64') {
            builder.append(`-p:RuntimeIdentifier=${Configuration.selectedDevice.runtime_id}`);
        }
        if (Configuration.isAndroid()) {
            builder.append('-p:EmbedAssembliesIntoApk=true');
            builder.append(`-p:AndroidSdkDirectory="${Configuration.getAndroidSdkDirectory()}"`);
        }
        if (Configuration.isWindows()) {
            builder.append('-p:WindowsPackageType=None');
            builder.append('-p:WinUISDKReferences=false');
        }
        
        return [ 
            new vscode.Task(
                new DotNetTaskDefinition(), 
                vscode.TaskScope.Workspace, 
                res.taskActionBuild, 
                res.extensionId,
                new vscode.ShellExecution(builder.build())
            )
        ];
    }
}
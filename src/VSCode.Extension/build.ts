import { Configuration } from './configuration';
import * as res from './resources';
import * as vscode from 'vscode';


class DotNetTaskDefinition implements vscode.TaskDefinition {
    public name: string = res.taskActionBuild;
    public type: string = res.taskIdBuild;
}

export class DotNetBuildTaskProvider implements vscode.TaskProvider {
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
            `-f:${framework}`,
            `-t:Build`
        ];
        
        if (!framework) {
            vscode.window.showErrorMessage(res.messageNoFrameworkFound);
            return [];
        }

        if (Configuration.isAndroid()) {
            command.push(`-p:EmbedAssembliesIntoApk=true`);
            command.push(`-p:AndroidSdkDirectory="${Configuration.androidSdk}"`);
        }
        if (Configuration.isIOS() && !Configuration.selectedDevice!.is_emulator) {
            command.push(`-p:RuntimeIdentifier=ios-arm64`);
        }
        if (Configuration.isMacCatalyst() && Configuration.selectedDevice!.is_arm) {
            command.push(`-p:RuntimeIdentifier=maccatalyst-arm64`);
        }
        
        return [ 
            new vscode.Task(
                new DotNetTaskDefinition(), 
                vscode.TaskScope.Workspace, 
                res.taskActionBuild, 
                res.extensionId,
                new vscode.ShellExecution(command.join(' '))
            )
        ];
    }
}
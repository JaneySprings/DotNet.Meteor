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

        if (Configuration.isAndroid()) {
            builder.append('-p:EmbedAssembliesIntoApk=true');
            builder.append(`-p:AndroidSdkDirectory="${Configuration.getAndroidSdkDirectory()}"`);
        }
        if (Configuration.isApple() && !Configuration.selectedDevice!.is_emulator) {
            builder.append('-p:RuntimeIdentifier=ios-arm64');
        }
        if (Configuration.isMacCatalyst() && Configuration.selectedDevice!.is_arm) {
            builder.append('-p:RuntimeIdentifier=maccatalyst-arm64');
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
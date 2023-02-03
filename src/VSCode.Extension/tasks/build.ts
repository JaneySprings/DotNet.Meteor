import { ProcessArgumentBuilder } from '../bridge';
import { Configuration } from '../configuration';
import * as res from '../resources';
import * as vscode from 'vscode';


interface DotNetTaskDefinition extends vscode.TaskDefinition {
    target: string;
    args?: string[];
}

export class DotNetTaskProvider implements vscode.TaskProvider {
    resolveTask(task: vscode.Task, token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task> { 
        const resolvedTask = this.getTask(task.definition as DotNetTaskDefinition);
        return resolvedTask ? resolvedTask : task;
    }
    provideTasks(token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task[]> {
        return this.getTasks();
    }

    private getTasks(): vscode.Task[] {
        const task = this.getTask({ 
            type: res.taskDefinitionId,
            target: res.taskDefinitionDefaultTarget,
        });
        return task ? [ task ] : [];
    }
    private getTask(definition: DotNetTaskDefinition): vscode.Task | undefined {
        Configuration.updateSelectedProject();
        if (!Configuration.validate())
            return undefined;
    
        const framework = Configuration.targetFramework();
        const builder = new ProcessArgumentBuilder('dotnet')
            .append(definition.target.toLowerCase())
            .appendQuoted(Configuration.selectedProject.path)
            .append(`-c:${Configuration.selectedTarget}`)
            .append(`-f:${framework}`);

        if (definition.target.toLowerCase() === 'build') 
            builder.append(`-t:Build`);
        
        if (!framework) {
            vscode.window.showErrorMessage(res.messageNoFrameworkFound);
            return undefined;
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

        definition.args?.forEach(arg => builder.override(arg));
        
        return new vscode.Task(
            definition, 
            vscode.TaskScope.Workspace, 
            definition.target, 
            res.extensionId,
            new vscode.ShellExecution(builder.build())
        );
    }
}
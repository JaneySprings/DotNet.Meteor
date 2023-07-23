import { ConfigurationController } from '../configuration';
import { ProcessArgumentBuilder } from '../bridge';
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
        if (!ConfigurationController.validate())
            return undefined;
    
        const framework = ConfigurationController.project!.frameworks?.find(it => it.includes(ConfigurationController.device!.platform!))
        const builder = new ProcessArgumentBuilder('dotnet')
            .append(definition.target.toLowerCase())
            .appendQuoted(ConfigurationController.project!.path)
            .append(`-p:Configuration=${ConfigurationController.target}`)
            .append(`-p:TargetFramework=${framework}`);

        if (definition.target.toLowerCase() === 'build') 
            builder.append(`-t:Build`);
        
        if (!framework) {
            vscode.window.showErrorMessage(res.messageNoFrameworkFound);
            return undefined;
        }

        if (ConfigurationController.device!.runtime_id) {
            builder.append(`-p:RuntimeIdentifier=${ConfigurationController.device!.runtime_id}`);
        }
        if (ConfigurationController.isAndroid()) {
            builder.append('-p:EmbedAssembliesIntoApk=true');
            builder.append(`-p:AndroidSdkDirectory="${ConfigurationController.androidSdkDirectory}"`);
        }
        if (ConfigurationController.isWindows()) {
            builder.append('-p:WindowsPackageType=None');
            builder.append('-p:WinUISDKReferences=false');
        }

        definition.args?.forEach(arg => builder.override(arg));
        
        return new vscode.Task(
            definition, 
            vscode.TaskScope.Workspace, 
            definition.target, 
            res.extensionId,
            new vscode.ShellExecution(builder.build()),
            `$${res.taskProblemMatcherId}`
        );
    }
}
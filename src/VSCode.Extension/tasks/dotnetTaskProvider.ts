import { ProcessArgumentBuilder } from '../processes/processArgumentBuilder';
import { ConfigurationController } from '../configurationController';
import { Target } from '../models/target';
import * as res from '../resources/constants';
import * as vscode from 'vscode';


export class DotNetTaskProvider implements vscode.TaskProvider {
    resolveTask(task: vscode.Task, token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task> { 
        return this.getTask(task.definition)
    }
    provideTasks(token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task[]> {
        return [
            this.getTask({ type: res.taskDefinitionId, target: res.taskDefinitionDefaultTarget })
        ];
    }

    private getTask(definition: vscode.TaskDefinition): vscode.Task {
        if (!definition.target)
            definition.target = res.taskDefinitionDefaultTarget;
    
        const builder = new ProcessArgumentBuilder('dotnet')
            .append(definition.target.toLowerCase());

        if (ConfigurationController.isActive()) {
            builder.appendQuoted(ConfigurationController.project!.path)
                .append(`-p:Configuration=${ConfigurationController.target}`)
                .append(`-p:TargetFramework=${ConfigurationController.getTargetFramework()}`)
                .conditional(`-p:RuntimeIdentifier=${ConfigurationController.device?.runtime_id}`, () => ConfigurationController.device?.runtime_id);

            if (ConfigurationController.isAndroid()) {
                builder.append('-p:EmbedAssembliesIntoApk=true');
                builder.appendFix(`-p:AndroidSdkDirectory="${ConfigurationController.androidSdkDirectory}"`);
                builder.conditional('-p:AndroidEnableProfiler=true', () => ConfigurationController.profiler);
            }
            if (ConfigurationController.isAppleMobile()) {
                builder.conditional('-p:MtouchDebug=true', () => ConfigurationController.target === Target.Debug);
                builder.conditional('-p:MtouchProfiling=true', () => ConfigurationController.profiler);
            }
            if (ConfigurationController.isMacCatalyst()) {
                builder.conditional('-p:Debug=true', () => ConfigurationController.target === Target.Debug);
                builder.conditional('-p:Profiling=true', () => ConfigurationController.profiler);
            }
            if (ConfigurationController.isWindows()) {
                builder.append('-p:WindowsPackageType=None');
                builder.append('-p:WinUISDKReferences=false');
            }
        }

        definition.args?.forEach((arg: string) => builder.override(arg));
        
        return new vscode.Task(
            definition, 
            vscode.TaskScope.Workspace, 
            /* It will be nice to use the 'definition.target' property 
            * but it's a huge breaking change for users */
            definition.target.charAt(0).toUpperCase() + definition.target.slice(1),
            res.extensionId,
            new vscode.ShellExecution(builder.build()),
            `$${res.taskProblemMatcherId}`
        );
    }
}
import { ProcessArgumentBuilder } from '../interop/processArgumentBuilder';
import { ConfigurationController } from '../configurationController';
import * as res from '../resources/constants';
import * as vscode from 'vscode';


export class DotNetTaskProvider implements vscode.TaskProvider {
    resolveTask(task: vscode.Task, token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task> { 
        return ConfigurationController.isActive() ? this.getTask(task.definition) : task;
    }
    provideTasks(token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task[]> {
        return ConfigurationController.isActive() ? [this.getTask({ type: res.taskDefinitionId, target: res.taskDefinitionDefaultTarget })] : undefined;
    }

    private getTask(definition: vscode.TaskDefinition): vscode.Task {
        if (!definition.target)
            definition.target = res.taskDefinitionDefaultTarget;
    
        const defaultTarget = definition.target === res.taskDefinitionDefaultTarget;
        const builder = new ProcessArgumentBuilder('dotnet')
            .append(definition.target.toLowerCase())
            .append(ConfigurationController.project!.path)
            .append(`-p:Configuration=${ConfigurationController.target}`)
            .append(`-p:TargetFramework=${ConfigurationController.getTargetFramework()}`)
            .conditional(`-p:RuntimeIdentifier=${ConfigurationController.device?.runtime_id}`, () => ConfigurationController.device?.runtime_id)

        if (ConfigurationController.isAndroid()) {
            builder.append(`-p:AndroidSdkDirectory=${ConfigurationController.androidSdkDirectory}`);
            builder.conditional('-p:EmbedAssembliesIntoApk=true', () => defaultTarget);
            builder.conditional('-p:AndroidEnableProfiler=true', () => ConfigurationController.profiler && defaultTarget);
        }
        if (ConfigurationController.isAppleMobile()) {
            builder.conditional('-p:_BundlerDebug=true', () => !ConfigurationController.profiler && defaultTarget);
            builder.conditional('-p:MtouchProfiling=true', () => ConfigurationController.profiler && defaultTarget);
        }
        if (ConfigurationController.isMacCatalyst()) {
            builder.conditional('-p:_BundlerDebug=true', () => !ConfigurationController.profiler && defaultTarget);
            builder.conditional('-p:Profiling=true', () => ConfigurationController.profiler && defaultTarget);
        }
        if (ConfigurationController.isWindows()) {
            builder.conditional('-p:WindowsPackageType=None', () => defaultTarget);
            builder.conditional('-p:WinUISDKReferences=false', () => defaultTarget);
        }

        definition.args?.forEach((arg: string) => builder.override(arg));
        
        return new vscode.Task(
            definition, 
            vscode.TaskScope.Workspace, 
            /* It will be nice to use the 'definition.target' property 
            * but it's a huge breaking change for users */
            definition.target.charAt(0).toUpperCase() + definition.target.slice(1),
            res.extensionId,
            new vscode.ShellExecution(builder.getCommand(), builder.getArguments()),
            `$${res.taskProblemMatcherId}`
        );
    }
}
import { ProcessArgumentBuilder } from '../interop/processArgumentBuilder';
import { ConfigurationController } from '../controllers/configurationController';
import * as res from '../resources/constants';
import * as vscode from 'vscode';


export class DotNetTaskProvider implements vscode.TaskProvider {
    resolveTask(task: vscode.Task, token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task> { 
        return ConfigurationController.isActive() ? this.getTask(task.definition) : task;
    }
    provideTasks(token: vscode.CancellationToken): vscode.ProviderResult<vscode.Task[]> {
        return ConfigurationController.isActive() ? [this.getTask({ type: res.taskDefinitionId })] : undefined;
    }

    private getTask(definition: vscode.TaskDefinition): vscode.Task {
        const builder = new ProcessArgumentBuilder('dotnet')
            .append('build')
            .append(ConfigurationController.project!.path)
            .append(`-p:Configuration=${ConfigurationController.configuration}`)
            .append(`-p:TargetFramework=${ConfigurationController.getTargetFramework()}`)
            .conditional(`-p:RuntimeIdentifier=${ConfigurationController.device?.runtime_id}`, () => ConfigurationController.device?.runtime_id)

        if (ConfigurationController.isAndroid()) {
            // TODO: FastDev
            // builder.conditional('-t:Run', () => !ConfigurationController.profiler)
            // builder.conditional('-p:AndroidAttachDebugger=true', () => !ConfigurationController.profiler && !ConfigurationController.noDebug);
            // builder.conditional(`-p:AndroidSdbTargetPort=${ConfigurationController.getDebuggingPort()}`, () => !ConfigurationController.profiler && !ConfigurationController.noDebug);
            // builder.conditional(`-p:AndroidSdbHostPort=${ConfigurationController.getDebuggingPort()}`, () => !ConfigurationController.profiler && !ConfigurationController.noDebug);
            // builder.conditional(`-p:AdbTarget=-s%20${ConfigurationController.device?.serial}`, () => !ConfigurationController.profiler);
            builder.append('-p:EmbedAssembliesIntoApk=true');
            builder.append('-p:CopyLocalLockFileAssemblies=true'); // because all DLLs inside app are broken
            builder.append(`-p:AndroidSdkDirectory=${ConfigurationController.androidSdkDirectory}`);
            builder.conditional('-p:AndroidEnableProfiler=true', () => ConfigurationController.profiler);
        }
        if (ConfigurationController.isAppleMobile()) {
            builder.conditional('-p:_BundlerDebug=true', () => !ConfigurationController.profiler);
            builder.conditional('-p:MtouchProfiling=true', () => ConfigurationController.profiler);
        }
        if (ConfigurationController.isMacCatalyst()) {
            builder.conditional('-p:_BundlerDebug=true', () => !ConfigurationController.profiler);
            builder.conditional('-p:Profiling=true', () => ConfigurationController.profiler);
        }
        if (ConfigurationController.isWindows()) {
            builder.append('-p:WindowsPackageType=None');
            builder.append('-p:WinUISDKReferences=false');
        }

        definition.args?.forEach((arg: string) => builder.override(arg));
        
        return new vscode.Task(
            definition, 
            vscode.TaskScope.Workspace,
            res.taskDefinitionDefaultTargetCapitalized,
            res.extensionId,
            new vscode.ShellExecution(builder.getCommand(), builder.getArguments()),
            `$${res.taskProblemMatcherId}`
        );
    }
}
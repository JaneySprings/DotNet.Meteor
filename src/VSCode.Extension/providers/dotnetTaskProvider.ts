import { ProcessArgumentBuilder } from '../interop/processArgumentBuilder';
import { ConfigurationController } from '../controllers/configurationController';
import { RemoteHostProvider } from '../features/removeHostProvider';
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
            builder.append(`-p:AndroidSdkDirectory=${ConfigurationController.androidSdkDirectory}`);
            builder.conditional('-p:AndroidIncludeDebugSymbols=true', () => !ConfigurationController.profiler);
            builder.conditional('-p:EmbedAssembliesIntoApk=true', () => ConfigurationController.profiler);
            builder.conditional('-p:AndroidEnableProfiler=true', () => ConfigurationController.profiler);
        }
        if (ConfigurationController.isAppleMobile()) {
            // TODO: https://github.com/xamarin/xamarin-macios/issues/21530
            // builder.conditional('-p:_BundlerDebug=true', () => !ConfigurationController.profiler);
            // builder.conditional('-p:MtouchProfiling=true', () => ConfigurationController.profiler);
            builder.append('-p:MtouchDebug=true');
            builder.conditional('-p:BuildIpa=true', () => !ConfigurationController.onMac);
        }
        if (ConfigurationController.isMacCatalyst()) {
            builder.conditional('-p:_BundlerDebug=true', () => !ConfigurationController.profiler);
            builder.conditional('-p:Profiling=true', () => ConfigurationController.profiler);
        }
        if (ConfigurationController.isWindows()) {
            builder.append('-p:WindowsPackageType=None');
            builder.append('-p:WinUISDKReferences=false');
        }

        if (ConfigurationController.isAppleMobile() && ConfigurationController.onWindows)
            RemoteHostProvider.feature.connect(builder);

        definition.args?.forEach((arg: string) => builder.override(arg));
        
        const task = new vscode.Task(
            definition, vscode.TaskScope.Workspace,
            res.taskDefinitionDefaultTargetCapitalized, res.extensionId,
            new vscode.ShellExecution(builder.getCommand(), builder.getArguments()), `$${res.taskProblemMatcherId}`
        );
        
        if (ConfigurationController.isAppleMobile() && ConfigurationController.onWindows)
            task.presentationOptions = { echo: false } /* Hide pair to mac commandline arguments */;

        return task;
    }
}
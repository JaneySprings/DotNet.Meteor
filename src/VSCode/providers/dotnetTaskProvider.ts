import { ProcessArgumentBuilder } from '../interop/processArgumentBuilder';
import { ConfigurationController } from '../controllers/configurationController';
import { Interop } from '../interop/interop';
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
            .append(`-p:TargetFramework=${ConfigurationController.targetFramework}`)
            .conditional(`-p:RuntimeIdentifier=${ConfigurationController.device?.runtime_id}`, () => ConfigurationController.device?.runtime_id)

        const remoteCoreclrTarget = ConfigurationController.getSettingOrDefault<string>(res.configIdRemoteCoreclrTarget);
        if (remoteCoreclrTarget !== undefined) {
            builder.append(`-p:CustomAfterMicrosoftCommonTargets="${Interop.customTargetsPath}"`);
            builder.append(`-p:RemoteCoreclrTargetDir=${remoteCoreclrTarget}`);
            builder.append('-p:UseMonoRuntime=false')
        }

        if (ConfigurationController.isAndroid()) {
            builder.append(`-p:AndroidSdkDirectory=${ConfigurationController.androidSdkDirectory}`);
            // TODO: https://github.com/dotnet/android/issues/9567
            builder.conditional(`-p:AdbTarget=-s%20${ConfigurationController.device?.serial}`, () => ConfigurationController.device?.serial);
        }
        if (ConfigurationController.isAppleMobile()) {
            builder.append('-p:EnableDiagnostics=true');
            builder.conditional('-p:BuildIpa=true', () => !ConfigurationController.onMac);
        }
        if (ConfigurationController.isMacCatalyst()) {
            builder.append('-p:EnableDiagnostics=True');
        }
        if (ConfigurationController.isWindows()) {
            builder.append('-p:WindowsPackageType=None');
            builder.append('-p:WinUISDKReferences=false');
        }

        definition.args?.forEach((arg: string) => builder.override(arg));
        return new vscode.Task(
            definition, vscode.TaskScope.Workspace,
            res.taskDefinitionDefaultTargetCapitalized, res.extensionId,
            new vscode.ShellExecution(builder.getCommand(), builder.getArguments()), `$${res.taskProblemMatcherId}`
        );
    }
}
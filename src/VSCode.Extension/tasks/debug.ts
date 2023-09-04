import { WorkspaceFolder, DebugConfiguration } from 'vscode';
import { ConfigurationController } from '../configuration';
import { UIController } from '../controller';
import { Target } from '../models';
import * as res from '../resources';
import * as vscode from 'vscode';


export class DotNetDebuggerConfiguration implements vscode.DebugConfigurationProvider {
	async resolveDebugConfiguration(folder: WorkspaceFolder | undefined, 
									config: DebugConfiguration, 
									token?: vscode.CancellationToken): Promise<DebugConfiguration | undefined> {
		
		if (!ConfigurationController.isActive())
			return undefined;
		if (!ConfigurationController.isValid())
			return undefined;

		if (!config.noDebug && ConfigurationController.isWindows()) {
			vscode.window.showErrorMessage(res.messageDebugNotSupportedWin);
			return undefined;
		}
		if (!config.noDebug && ConfigurationController.target === Target.Release) {
			vscode.window.showErrorMessage(res.messageDebugNotSupported);
			return undefined;
		}

		const targetDevice = { ...ConfigurationController.device };
		const targetProject = { ...ConfigurationController.project };

		if (config.device !== undefined) 
			UIController.performSelectDevice(UIController.devices.find(d => d.name === config.device));
		if (config.runtime !== undefined)
			targetDevice!.runtime_id = config.runtime;

		if (!config.type && !config.request && !config.name) {
			config.preLaunchTask = `${res.extensionId}: ${res.taskDefinitionDefaultTargetCapitalized}`
			config.name = res.debuggerMeteorTitle;
			config.type = res.debuggerMeteorId;
			config.request = 'launch';
		}
		
        config['selected_device'] = targetDevice;
		config['selected_project'] = targetProject;
		config['selected_target'] = ConfigurationController.target;
		config['debugging_port'] = ConfigurationController.getDebuggingPort();
		config['uninstall_app'] = ConfigurationController.getUninstallAppOption();
		config['reload_host'] = ConfigurationController.getReloadHostPort();
		
        return config;
	}
}
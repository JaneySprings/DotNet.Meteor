import { WorkspaceFolder, DebugConfiguration } from 'vscode';
import { Configuration } from '../configuration';
import { UIController } from '../controller';
import { Target } from '../models';
import * as res from '../resources';
import * as vscode from 'vscode';


export class DotNetDebuggerConfiguration implements vscode.DebugConfigurationProvider {
	async resolveDebugConfiguration(folder: WorkspaceFolder | undefined, 
									config: DebugConfiguration, 
									token?: vscode.CancellationToken): Promise<DebugConfiguration | undefined> {
		
		if (!Configuration.validate())
			return undefined;

		const targetDevice = { ...Configuration.device };
		const targetProject = { ...Configuration.project };
		
		if (!config.noDebug && Configuration.isWindows()) {
			vscode.window.showErrorMessage(res.messageDebugNotSupportedWin);
			return undefined;
		}
		if (!config.noDebug && Configuration.target === Target.Release) {
			vscode.window.showErrorMessage(res.messageDebugNotSupported);
			return undefined;
		}

		if (config.device !== undefined) 
			UIController.performSelectDevice(UIController.devices.find(d => d.name === config.device));
		if (config.runtime !== undefined)
			targetDevice!.runtime_id = config.runtime;

		if (!config.type && !config.request && !config.name) {
			config.preLaunchTask = `${res.extensionId}: ${res.taskDefinitionDefaultTarget}`
			config.name = res.debuggerMeteorTitle;
			config.type = res.debuggerMeteorId;
			config.request = 'launch';
		}
		
        config['selected_device'] = targetDevice;
		config['selected_project'] = targetProject;
		config['selected_target'] = Configuration.target;
		config['debugging_port'] = Configuration.getDebuggingPort();
		
        return config;
	}
}
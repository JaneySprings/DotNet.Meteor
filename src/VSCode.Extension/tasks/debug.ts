import { WorkspaceFolder, DebugConfiguration } from 'vscode';
import { Configuration } from '../configuration';
import { Target } from '../models';
import * as res from '../resources';
import * as vscode from 'vscode';


export class DotNetDebuggerConfiguration implements vscode.DebugConfigurationProvider {
	async resolveDebugConfiguration(folder: WorkspaceFolder | undefined, 
									config: DebugConfiguration, 
									token?: vscode.CancellationToken): Promise<DebugConfiguration | undefined> {
		
		if (!Configuration.validate()) return undefined;
		if (config.noDebug === undefined) {
			if (Configuration.isWindows()) {
				vscode.window.showErrorMessage(res.messageDebugNotSupportedWin);
				return undefined;
			}
			if (Configuration.target === Target.Release) {
				vscode.window.showErrorMessage(res.messageDebugNotSupported);
				return undefined;
			}
		}
		
		if (!config.type && !config.request && !config.name) {
			config.preLaunchTask = `${res.extensionId}: ${res.taskDefinitionDefaultTarget}`
			config.name = res.debuggerMeteorTitle;
			config.type = res.debuggerMeteorId;
			config.request = 'launch';
		}
		
		config['selected_project'] = Configuration.project;
        config['selected_device'] = Configuration.device;
		config['selected_target'] = Configuration.target;
		config['debugging_port'] = Configuration.getDebuggingPort();
		
        return config;
	}
}
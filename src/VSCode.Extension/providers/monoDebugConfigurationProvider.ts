import { ConfigurationController } from '../configurationController';
import { WorkspaceFolder, DebugConfiguration } from 'vscode';
import * as res from '../resources/constants';
import * as vscode from 'vscode';

export class MonoDebugConfigurationProvider implements vscode.DebugConfigurationProvider {
	async resolveDebugConfiguration(folder: WorkspaceFolder | undefined, 
									config: DebugConfiguration, 
									token?: vscode.CancellationToken): Promise<DebugConfiguration | undefined> {
		
		if (!ConfigurationController.isActive())
			return undefined;
		if (!ConfigurationController.isValid())
			return undefined;

		ConfigurationController.profiler = config.profilerMode;
		if (!config.noDebug && ConfigurationController.profiler) {
			vscode.window.showErrorMessage(res.messageDebugNotSupported, { modal: true });
			return undefined;
		}
		if (!config.noDebug && ConfigurationController.isWindows()) {
			vscode.window.showErrorMessage(res.messageDebugNotSupportedWin, { modal: true });
			return undefined;
		}

		if (!config.type && !config.request && !config.name) {
			config.preLaunchTask = `${res.extensionId}: ${res.taskDefinitionDefaultTargetCapitalized}`
			config.name = res.debuggerMeteorTitle;
			config.type = res.debuggerMeteorId;
			config.request = 'launch';
		}
		
		if (config.project == undefined)
			config.project = ConfigurationController.project;
		if (config.configuration == undefined)
			config.configuration = ConfigurationController.target;
		if (config.device == undefined)
        	config.device = ConfigurationController.device;

		config.skipDebug = config.noDebug ?? false;
		config.debuggingPort = ConfigurationController.getDebuggingPort();
		config.uninstallApp = ConfigurationController.getUninstallAppOption();
		config.reloadHost = ConfigurationController.getReloadHostPort();
		config.profilerPort = ConfigurationController.getProfilerPort();
		config.debuggerOptions = ConfigurationController.getDebuggerOptions();
		
        return config;
	}
}
import { ConfigurationController } from '../configurationController';
import { StatusBarController } from '../statusbarController';
import { WorkspaceFolder, DebugConfiguration } from 'vscode';
import { Target } from '../models/target';
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

		ConfigurationController.profiler = config['profilerMode'];
		if (!config.noDebug && (ConfigurationController.target === Target.Release || ConfigurationController.profiler)) {
			vscode.window.showErrorMessage(res.messageDebugNotSupported);
			return undefined;
		}
		if (!config.noDebug && ConfigurationController.isWindows()) {
			vscode.window.showErrorMessage(res.messageDebugNotSupportedWin);
			return undefined;
		}

		const targetDevice = { ...ConfigurationController.device };
		const targetProject = { ...ConfigurationController.project };

		if (config.device !== undefined) 
			StatusBarController.performSelectDevice(StatusBarController.devices.find(d => d.name === config.device));
		if (config.runtime !== undefined)
			targetDevice!.runtime_id = config.runtime;

		if (!config.type && !config.request && !config.name) {
			config.preLaunchTask = `${res.extensionId}: ${res.taskDefinitionDefaultTargetCapitalized}`
			config.name = res.debuggerMeteorTitle;
			config.type = res.debuggerMeteorId;
			config.request = 'launch';
		}
		
        config['selectedDevice'] = targetDevice;
		config['selectedProject'] = targetProject;
		config['selectedTarget'] = ConfigurationController.target;
		config['debuggingPort'] = ConfigurationController.getDebuggingPort();
		config['uninstallApp'] = ConfigurationController.getUninstallAppOption();
		config['reloadHost'] = ConfigurationController.getReloadHostPort();
		config['profilerPort'] = ConfigurationController.getProfilerPort();
		config['debuggerOptions'] = ConfigurationController.getDebuggerOptions();
		
        return config;
	}
}
import { ConfigurationController } from '../controllers/configurationController';
import * as res from '../resources/constants';
import * as vscode from 'vscode';

export class ProfileConfigurationProvider implements vscode.DebugConfigurationProvider {
	async resolveDebugConfiguration(folder: vscode.WorkspaceFolder | undefined, 
									config: vscode.DebugConfiguration, 
									token?: vscode.CancellationToken): Promise<vscode.DebugConfiguration | undefined> {
		
		ConfigurationController.profiler = true;
		ConfigurationController.noDebug = config.noDebug;

		if (!ConfigurationController.isActive())
			return undefined;
		if (!ConfigurationController.isValid())
			return undefined;
		
		if (!config.type && !config.request && !config.name) {
			config.preLaunchTask = `${res.extensionId}: ${res.taskDefinitionDefaultTargetCapitalized}`
			config.name = res.profilerMeteorTitle;
			config.type = res.profilerMeteorId;
			config.request = 'launch';
		}
		//TODO: Legacy (Remove in future versions)
		if (config.type !== res.profilerMeteorId)
			config.type = res.profilerMeteorId;

		if (config.project === undefined)
			config.project = ConfigurationController.project;
		if (config.configuration === undefined)
			config.configuration = ConfigurationController.configuration;
		if (config.device === undefined)
        	config.device = ConfigurationController.device;
		if (config.program === undefined)
			config.program = ConfigurationController.getProgramPath(config.project, config.configuration, config.device);

		config.profilerPort = ConfigurationController.getProfilerPort();

        return config;
	}
}
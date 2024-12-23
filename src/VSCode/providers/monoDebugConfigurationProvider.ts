import { ConfigurationController } from '../controllers/configurationController';
import { ProfileConfigurationProvider } from './profileConfigurationProvider';
import { ExternalTypeResolver } from '../features/externalTypeResolver';
import * as res from '../resources/constants';
import * as vscode from 'vscode';

export class MonoDebugConfigurationProvider implements vscode.DebugConfigurationProvider {
	async resolveDebugConfiguration(folder: vscode.WorkspaceFolder | undefined, 
									config: vscode.DebugConfiguration, 
									token?: vscode.CancellationToken): Promise<vscode.DebugConfiguration | undefined> {
		
		//TODO: Legacy (Remove in future versions)
		if (config.profilerMode) {
			const provider = new ProfileConfigurationProvider();
			return provider.resolveDebugConfiguration(folder, config, token);
		}

		ConfigurationController.profiler = false;
		ConfigurationController.noDebug = config.noDebug;
		
		if (!ConfigurationController.isActive())
			return undefined;
		if (!ConfigurationController.isValid())
			return undefined;

		if (!config.type && !config.request && !config.name) {
			config.preLaunchTask = `${res.extensionId}: ${res.taskDefinitionDefaultTargetCapitalized}`
			config.name = res.debuggerMeteorTitle;
			config.type = res.debuggerMeteorId;
			config.request = 'launch';
		}
		if (config.project === undefined)
			config.project = ConfigurationController.project;
		if (config.configuration === undefined)
			config.configuration = ConfigurationController.configuration;
		if (config.device === undefined)
        	config.device = ConfigurationController.device;
		if (config.program === undefined)
			config.program = ConfigurationController.getProgramPath(config.project, config.configuration, config.device);
		if (config.assets === undefined)
			config.assets = ConfigurationController.getAssetsPath(config.project, config.configuration, config.device);

		if (ConfigurationController.isVsdbgRequired()) {
			config.type = res.debuggerVsdbgId;
			config.project = undefined;
			config.configuration = undefined;
			config.device = undefined;
			return ConfigurationController.getVsdbgOptions(config);
		}

		config.transportId = ExternalTypeResolver.feature.transportId;
		config.skipDebug = ConfigurationController.noDebug;
		config.debuggingPort = ConfigurationController.getDebuggingPort();
		config.uninstallApp = ConfigurationController.getUninstallAppOption();
		config.reloadHost = ConfigurationController.getReloadHostPort();
		config.debuggerOptions = ConfigurationController.getDebuggerOptions();
		
        return config;
	}
}
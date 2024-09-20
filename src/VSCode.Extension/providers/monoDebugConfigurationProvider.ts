import { ConfigurationController } from '../controllers/configurationController';
import { WorkspaceFolder, DebugConfiguration } from 'vscode';
import * as res from '../resources/constants';
import * as vscode from 'vscode';
import { InteropController } from '../controllers/interopController';

export class MonoDebugConfigurationProvider implements vscode.DebugConfigurationProvider {
	async resolveDebugConfiguration(folder: WorkspaceFolder | undefined, 
									config: DebugConfiguration, 
									token?: vscode.CancellationToken): Promise<DebugConfiguration | undefined> {
		
		if (!ConfigurationController.isActive())
			return undefined;
		if (!ConfigurationController.isValid())
			return undefined;

		if (!config.noDebug && config.profilerMode) {
			vscode.window.showErrorMessage(res.messageDebugNotSupported, { modal: true });
			return undefined;
		}

		if (!config.type && !config.request && !config.name) {
			config.preLaunchTask = `${res.extensionId}: ${res.taskDefinitionDefaultTarget}`
			config.name = res.debuggerVsdbgTitle;
			config.type = res.debuggerVsdbgId;
			config.request = 'launch';
		}
		
		if (config.project === undefined)
			config.project = ConfigurationController.project;
		if (config.configuration === undefined)
			config.configuration = ConfigurationController.target;
		if (config.device === undefined) {
        	config.device = ConfigurationController.device;
			
			if (ConfigurationController.isAndroid() && ConfigurationController.device !== undefined && ConfigurationController.device.is_emulator && !ConfigurationController.device.is_running) {
				ConfigurationController.device.serial = await InteropController.runEmulator(ConfigurationController.device?.name ?? 'null');
				ConfigurationController.device.is_running = ConfigurationController.device.serial !== undefined;
			}
		}

		if (config.program === undefined)
			config.program = ConfigurationController.getProgramPath(config.project, config.configuration, config.device);

		//TODO: check
		config.launchConfigurationId = 'vscode-maui';
		config.project = ConfigurationController.project?.path;
		config.platform = ConfigurationController.device?.platform;
		config.debugTarget = ConfigurationController.device?.name;
		config.isEmulator = ConfigurationController.device?.is_emulator;
		config.platformArchitecture = 'x86_64';
		config.isMaui = true;
		config.isMultiplatform = true;
		config.targetFramework = ConfigurationController.getTargetFramework();
		config.device = ConfigurationController.device?.serial;
		config.debugPort = ConfigurationController.getDebuggingPort();
		config.useVSDbg = 1;
		config.mauiVersion = '9.0.0-preview.7.24407.4';
		config.dotnetVersion = '9.0.0-preview.7.24407.12';
		config.monoDebuggerOptions = {
			ip: '127.0.0.1',
			port: ConfigurationController.getDebuggingPort(),
			platform: ConfigurationController.device?.platform,
			isServer: false,
			assetsPath: "C:\\Users\\nromanov\\Storage\\MauiTestApp\\obj\\Debug\\net9.0-android\\android\\assets\\;C:\\Users\\nromanov\\Storage\\MauiTestApp\\obj\\Debug\\net9.0-android\\android\\assets\\x86_64\\"
		},
		config._mauiBuildOutputPropsFile = "C:\\Users\\nromanov\\AppData\\Local\\Temp\\dotnet-maui\\maui-vsc-7ed0e321-1511-488f-acc5-876e19a4fc11.json";
		config.androidLocalTunnels = [
			{
				deviceId: ConfigurationController.device?.serial,
				port: ConfigurationController.getDebuggingPort()+1,
			}
		]
		// config.project = undefined;
		// config.configuration = undefined;
		// config.device = undefined;
		// Replace it to:
		// ConfigurationController.setupDebuggerOptions(config);

		// if (config.profilerMode !== undefined) {
		// 	ConfigurationController.profiler = config.profilerMode;
		// 	config.profilerPort = ConfigurationController.getProfilerPort();
		// }
		
        return config;
	}
}
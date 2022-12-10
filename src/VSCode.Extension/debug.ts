import { WorkspaceFolder, DebugConfiguration } from 'vscode';
import { Configuration } from './configuration';
import { Controller } from './controller';
import { Target } from './models';
import * as vscode from 'vscode';
import { CommandLine } from './bridge';


export class DotNetDebuggerConfiguration implements vscode.DebugConfigurationProvider {
	async resolveDebugConfiguration(folder: WorkspaceFolder | undefined, 
									config: DebugConfiguration, 
									token?: vscode.CancellationToken): Promise<DebugConfiguration | undefined> {
		
		if (config.noDebug === undefined && Configuration.selectedTarget === Target.Release) {
			vscode.window.showErrorMessage("Debugging is not supported in release configuration");
			return undefined;
		}
		
		const actualDevice = CommandLine.deviceInfo(Configuration.selectedDevice);

		Configuration.updateDebuggingPort();
		Configuration.selectedDevice = actualDevice;
		Controller.isDebugging = true;

		if (!config.type && !config.request && !config.name) {
			config.type = 'dotnet-meteor.debug';
			config.name = 'Debug .NET Mobile App';
			config.request = 'launch';
			config.preLaunchTask = 'dotnet-meteor: build';
		}
		
		config['selected_project'] = Configuration.selectedProject;
        config['selected_device'] = Configuration.selectedDevice;
		config['selected_target'] = Configuration.selectedTarget;
		config['debugging_port'] = Configuration.debuggingPort;
		
        return config;
	}
}
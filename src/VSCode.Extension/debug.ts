import { WorkspaceFolder, DebugConfiguration } from 'vscode';
import { Configuration } from './configuration';
import { CommandLine } from './bridge';
import { Target } from './models';
import * as res from './resources';
import * as vscode from 'vscode';


export class DotNetDebuggerConfiguration implements vscode.DebugConfigurationProvider {
	async resolveDebugConfiguration(folder: WorkspaceFolder | undefined, 
									config: DebugConfiguration, 
									token?: vscode.CancellationToken): Promise<DebugConfiguration | undefined> {
		
		if (config.noDebug === undefined && Configuration.selectedTarget === Target.Release) {
			vscode.window.showErrorMessage(res.messageDebugNotSupported);
			return undefined;
		}
		
		const actualDevice = CommandLine.deviceInfo(Configuration.selectedDevice);

		Configuration.selectedDevice = actualDevice;
		Configuration.updateDebuggingPort();

		if (!config.type && !config.request && !config.name) {
			config.preLaunchTask = res.taskTitleBuild;
			config.name = res.debuggerMeteorTitle;
			config.type = res.debuggerMeteorId;
			config.request = 'launch';
		}
		
		config['selected_project'] = Configuration.selectedProject;
        config['selected_device'] = Configuration.selectedDevice;
		config['selected_target'] = Configuration.selectedTarget;
		config['debugging_port'] = Configuration.debuggingPort;
		
        return config;
	}
}
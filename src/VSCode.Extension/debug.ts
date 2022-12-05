import { WorkspaceFolder, DebugConfiguration } from 'vscode';
import { Configuration } from './configuration';
import { Controller } from './controller';
import { Target } from './models';
import * as vscode from 'vscode';


export class DotNetDebuggerConfiguration implements vscode.DebugConfigurationProvider {
	async resolveDebugConfiguration(folder: WorkspaceFolder | undefined, 
									config: DebugConfiguration, 
									token?: vscode.CancellationToken): Promise<DebugConfiguration | undefined> {
		
		if (config.noDebug === undefined && Configuration.selectedTarget === Target.Release) {
			vscode.window.showErrorMessage("Debugging is not supported in release configuration");
			return undefined;
		}

		Configuration.updateDebuggingPort();
		Controller.isDebugging = true;
		
		config['selected_project'] = Configuration.selectedProject;
        config['selected_device'] = Configuration.selectedDevice;
		config['selected_target'] = Configuration.selectedTarget;
		config['debugging_port'] = Configuration.debuggingPort;
		
        return config;
	}
}
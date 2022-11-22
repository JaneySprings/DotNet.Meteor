import { WorkspaceFolder, DebugConfiguration } from 'vscode';
import { Configuration } from './configuration';
import { ViewController } from './controller';
import * as vscode from 'vscode';


export class DotNetDebuggerConfiguration implements vscode.DebugConfigurationProvider {
	async resolveDebugConfiguration(folder: WorkspaceFolder | undefined, 
									config: DebugConfiguration, 
									token?: vscode.CancellationToken): Promise<DebugConfiguration> {

		Configuration.updateDebuggingPort();
		ViewController.fetchDevices();
		ViewController.isDebugging = true;
		
		config['selected_project'] = Configuration.selectedProject;
        config['selected_device'] = Configuration.selectedDevice;
		config['debugging_port'] = Configuration.debuggingPort;
		
        return config;
	}
}
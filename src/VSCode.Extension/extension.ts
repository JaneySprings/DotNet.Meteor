import * as vscode from 'vscode';
import { Configuration } from './configuration';
import { Command, taskProviderType, debuggerType } from './constants';
import { Interface } from './interface';
import { DotNetTaskProvider } from './tasks';
import { DotNetDebuggerConfiguration } from './debug';


export function activate(context: vscode.ExtensionContext) {
	Interface.activate();

	if (vscode.workspace.workspaceFolders === undefined) 
		return;

	Configuration.fetchWorkspace();
	Configuration.fetchDevices();
	
	if (Configuration.workspaceProjects.length === 0)
		return;
	
	Configuration.performSelectDefaults();

	context.subscriptions.push(vscode.commands.registerCommand(Command.selectProject, Interface.showQuickPickProject));
	context.subscriptions.push(vscode.commands.registerCommand(Command.selectTarget, Interface.showQuickPickTarget));
	context.subscriptions.push(vscode.commands.registerCommand(Command.selectDevice, Interface.showQuickPickDevice));
	// Execution
	context.subscriptions.push(vscode.debug.registerDebugConfigurationProvider(debuggerType, new DotNetDebuggerConfiguration()));
	vscode.tasks.registerTaskProvider(taskProviderType, new DotNetTaskProvider());
}

export function deactivate() {}
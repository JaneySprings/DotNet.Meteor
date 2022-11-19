import * as vscode from 'vscode';
import { Configuration } from './configuration';
import { Command } from './constants';
import { Interface } from './interface';


export function activate(context: vscode.ExtensionContext) {
	Interface.activate();

	if (vscode.workspace.workspaceFolders === undefined) 
		return;

	Configuration.fetchWorkspace();
	Configuration.fetchDevices();
	
	if (Configuration.workspaceProjects.length === 0)
		return;
	
	Configuration.selectDefaults();

	context.subscriptions.push(vscode.commands.registerCommand(Command.selectProject, Interface.showQuickPickProject));
	context.subscriptions.push(vscode.commands.registerCommand(Command.selectTarget, Interface.showQuickPickTarget));
	context.subscriptions.push(vscode.commands.registerCommand(Command.selectDevice, Interface.showQuickPickDevice));
}

export function deactivate() {}
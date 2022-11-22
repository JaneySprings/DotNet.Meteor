import * as vscode from 'vscode';
import { Command, taskProviderType, debuggerType } from './constants';
import { ViewController } from './controller';
import { DotNetTaskProvider } from './tasks';
import { DotNetDebuggerConfiguration } from './debug';


export function activate(context: vscode.ExtensionContext) {
	ViewController.activate();

	if (vscode.workspace.workspaceFolders === undefined) 
		return;

	ViewController.fetchWorkspace();
	ViewController.fetchDevices();
	
	if (ViewController.workspaceProjects.length === 0)
		return;
	
	ViewController.performSelectDefaults();

	context.subscriptions.push(vscode.commands.registerCommand(Command.selectProject, ViewController.showQuickPickProject));
	context.subscriptions.push(vscode.commands.registerCommand(Command.selectTarget, ViewController.showQuickPickTarget));
	context.subscriptions.push(vscode.commands.registerCommand(Command.selectDevice, ViewController.showQuickPickDevice));
	// Execution
	context.subscriptions.push(vscode.debug.registerDebugConfigurationProvider(debuggerType, new DotNetDebuggerConfiguration()));
	context.subscriptions.push(vscode.debug.onDidTerminateDebugSession(() => ViewController.isDebugging = false));
	
	vscode.tasks.registerTaskProvider(taskProviderType, new DotNetTaskProvider());
}

export function deactivate() {}
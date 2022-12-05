import { Command, taskProviderType, debuggerType } from './constants';
import { DotNetDebuggerConfiguration } from './debug';
import { Controller } from './controller';
import { DotNetTaskProvider } from './tasks';
import { CommandLine } from './bridge';
import { Target } from './models';
import * as vscode from 'vscode';


export function activate(context: vscode.ExtensionContext) {
	if (vscode.workspace.workspaceFolders === undefined) 
		return;
	
	Controller.activate();
	CommandLine.analyzeWorkspaceAsync(items => {
		if (items.length === 0) 
			Controller.deactivate();
		
		Controller.workspaceProjects = items;
		Controller.performSelectProject(items[0]);
		Controller.performSelectTarget(Target.Debug);
	});
	CommandLine.mobileDevicesAsync(items => {
		if (items.length === 0) 
			Controller.deactivate();

		Controller.mobileDevices = items
		Controller.performSelectDevice(items[0]);
	});
	
	context.subscriptions.push(vscode.commands.registerCommand(Command.selectProject,Controller.showQuickPickProject));
	context.subscriptions.push(vscode.commands.registerCommand(Command.selectTarget, Controller.showQuickPickTarget));
	context.subscriptions.push(vscode.commands.registerCommand(Command.selectDevice, Controller.showQuickPickDevice));
	
	context.subscriptions.push(vscode.debug.registerDebugConfigurationProvider(debuggerType, new DotNetDebuggerConfiguration()));
	context.subscriptions.push(vscode.debug.onDidTerminateDebugSession(() => Controller.isDebugging = false));
	
	vscode.tasks.registerTaskProvider(taskProviderType, new DotNetTaskProvider());
}

export function deactivate() {}
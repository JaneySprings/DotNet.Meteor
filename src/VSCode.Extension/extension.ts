import { Command, debuggerType } from './constants';
import { DotNetDebuggerConfiguration } from './debug';
import { Controller } from './controller';
import { DotNetPublishTaskProvider } from './publish';
import { DotNetBuildTaskProvider } from './build';
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
	
	vscode.tasks.registerTaskProvider(DotNetPublishTaskProvider.type, new DotNetPublishTaskProvider());
	vscode.tasks.registerTaskProvider(DotNetBuildTaskProvider.type, new DotNetBuildTaskProvider());
	vscode.debug.registerDebugConfigurationProvider(debuggerType, new DotNetDebuggerConfiguration());
}

export function deactivate() {}
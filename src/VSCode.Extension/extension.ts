import { DotNetDebuggerConfiguration } from './tasks/debug';
import { DotNetTaskProvider } from './tasks/build';
import { Controller } from './controller';
import { CommandLine } from './bridge';
import { Target } from './models';
import * as res from './resources';
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
	
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveProject, Controller.showQuickPickProject));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveConfiguration, Controller.showQuickPickTarget));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveDevice, Controller.showQuickPickDevice));
	
	vscode.debug.onDidStartDebugSession(() => vscode.commands.executeCommand(res.commandIdFocusOnDebug));
	vscode.debug.registerDebugConfigurationProvider(res.debuggerMeteorId, new DotNetDebuggerConfiguration());
	vscode.tasks.registerTaskProvider(res.taskDefinitionId, new DotNetTaskProvider());
}
import { DotNetDebuggerConfiguration } from './tasks/debug';
import { DotNetTaskProvider } from './tasks/build';
import { Configuration } from './configuration';
import { UIController } from './controller';
import { CommandLine } from './bridge';
import { Target } from './models';
import * as res from './resources';
import * as vscode from 'vscode';


export function activate(context: vscode.ExtensionContext) {
	if (vscode.workspace.workspaceFolders === undefined) 
		return;
	
	UIController.activate(context);
	analyzeWorkspace();
	analyzeDevices();

	/* Commands */
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveProject, UIController.showQuickPickProject));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveConfiguration, UIController.showQuickPickTarget));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveDevice, UIController.showQuickPickDevice));
	/* Providers */
	context.subscriptions.push(vscode.debug.registerDebugConfigurationProvider(res.debuggerMeteorId, new DotNetDebuggerConfiguration()));
	context.subscriptions.push(vscode.tasks.registerTaskProvider(res.taskDefinitionId, new DotNetTaskProvider()));
	/* Events */
	context.subscriptions.push(vscode.debug.onDidStartDebugSession(() => vscode.commands.executeCommand(res.commandIdFocusOnDebug)));
	context.subscriptions.push(vscode.workspace.onDidChangeWorkspaceFolders(analyzeWorkspace));
}


function analyzeWorkspace() {
	CommandLine.analyzeWorkspaceAsync(items => {
		UIController.workspaceProjects = items;
		if (!Configuration.selectedProject) 
			UIController.performSelectTarget(Target.Debug);
		if (!Configuration.selectedProject || !items.includes(Configuration.selectedProject)) 
			UIController.performSelectProject(items[0]);
	});
}

function analyzeDevices() {
	CommandLine.mobileDevicesAsync(items => {
		UIController.mobileDevices = items
		UIController.performSelectDevice(items[0]);
	});
}
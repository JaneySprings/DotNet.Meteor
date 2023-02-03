import { DotNetDebuggerConfiguration } from './tasks/debug';
import { DotNetTaskProvider } from './tasks/build';
import { Configuration } from './configuration';
import { UIController } from './controller';
import { CommandInterface } from './bridge';
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
	CommandInterface.analyzeWorkspaceAsync(items => {
		if (items.length === 0) {
			UIController.deactivate();
			return;
		}

		UIController.workspaceProjects = items;
		if (!Configuration.selectedProject) 
			UIController.performSelectTarget();
		if (!Configuration.selectedProject || !items.some(it => it.path === Configuration.selectedProject.path)) 
			UIController.performSelectProject();
	});
}

function analyzeDevices() {
	CommandInterface.mobileDevicesAsync(items => {
		if (items.length === 0) {
			UIController.deactivate();
			return;
		}

		UIController.mobileDevices = items;
		UIController.performSelectDevice();
	});
}
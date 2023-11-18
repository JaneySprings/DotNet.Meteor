import { DotNetDebuggerConfiguration } from './tasks/debug';
import { ConfigurationController } from './configuration';
import { DotNetTaskProvider } from './tasks/build';
import { XamlController } from './xaml/service';
import { CommandController } from './bridge';
import { UIController } from './controller';
import { PublicExports } from './exports';
import { StateController } from './cache';
import * as res from './resources';
import * as vscode from 'vscode';


export function activate(context: vscode.ExtensionContext): PublicExports | undefined {
	if (vscode.workspace.workspaceFolders === undefined) 
		return undefined;

	if (!CommandController.activate(context))
		return undefined;
	
	const exports = new PublicExports();
	
	ConfigurationController.activate(context);
	StateController.activate(context);
	XamlController.activate(context);
	UIController.activate(context);
	UIController.update();

	/* Commands */
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveProject, UIController.showQuickPickProject));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveConfiguration, UIController.showQuickPickTarget));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveDevice, UIController.showQuickPickDevice));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveTargetFramework, () => ConfigurationController.getTargetFramework()));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveConfiguration, () => ConfigurationController.target));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveProjectPath, () => ConfigurationController.project?.path));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveDeviceName, () => ConfigurationController.device?.name));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveDeviceSerial, () => ConfigurationController.device?.serial));
	/* Providers */
	context.subscriptions.push(vscode.debug.registerDebugConfigurationProvider(res.debuggerMeteorId, new DotNetDebuggerConfiguration()));
	context.subscriptions.push(vscode.tasks.registerTaskProvider(res.taskDefinitionId, new DotNetTaskProvider()));
	/* Events */
	context.subscriptions.push(vscode.workspace.onDidChangeWorkspaceFolders(UIController.update));
	context.subscriptions.push(vscode.workspace.onDidSaveTextDocument(ev => {
		if (ev.fileName.endsWith('proj') || ev.fileName.endsWith('.props'))
			UIController.update();
	}));
	context.subscriptions.push(vscode.tasks.onDidEndTask(ev => {
		if (ev.execution.task.definition.type.includes(res.taskDefinitionId))
			XamlController.regenerate();
	}));

	return exports;
}

export function deactivate() {
	StateController.deactivate();
}
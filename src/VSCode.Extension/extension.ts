import { MonoDebugConfigurationProvider } from './tasks/monoDebugConfigurationProvider';
import { DotNetTaskProvider } from './tasks/dotnetTaskProvider';
import { ConfigurationController } from './configurationController';
import { StatusBarController } from './statusbarController';
import { CommandController } from './commandController';
import { StateController } from './stateController';
import { XamlController } from './xaml/xamlController';
import { PublicExports } from './publicExports';
import * as res from './resources/constants';
import * as vscode from 'vscode';


export function activate(context: vscode.ExtensionContext): PublicExports | undefined {
	if (vscode.workspace.workspaceFolders === undefined) 
		return undefined;

	const exports = new PublicExports();
	
	CommandController.activate(context);
	ConfigurationController.activate(context);
	StateController.activate(context);
	XamlController.activate(context);
	StatusBarController.activate(context);
	StatusBarController.update();

	/* Commands */
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveProject, StatusBarController.showQuickPickProject));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveConfiguration, StatusBarController.showQuickPickTarget));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveDevice, StatusBarController.showQuickPickDevice));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveTargetFramework, () => ConfigurationController.getTargetFramework()));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveConfiguration, () => ConfigurationController.target));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveProjectPath, () => ConfigurationController.project?.path));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveDeviceName, () => ConfigurationController.device?.name));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveDeviceSerial, () => ConfigurationController.device?.serial));
	/* Providers */
	context.subscriptions.push(vscode.debug.registerDebugConfigurationProvider(res.debuggerMeteorId, new MonoDebugConfigurationProvider()));
	context.subscriptions.push(vscode.tasks.registerTaskProvider(res.taskDefinitionId, new DotNetTaskProvider()));
	/* Events */
	context.subscriptions.push(vscode.workspace.onDidSaveTextDocument(ev => {
		if (ev.fileName.endsWith('.xaml') && vscode.debug.activeDebugSession?.configuration.type === res.debuggerMeteorId && !ConfigurationController.profiler)
			vscode.debug.activeDebugSession.customRequest('hotReload', { filePath: ev.fileName });
	}));
	context.subscriptions.push(vscode.workspace.onDidChangeWorkspaceFolders(StatusBarController.update));
	context.subscriptions.push(vscode.workspace.onDidSaveTextDocument(ev => {
		if (ev.fileName.endsWith('proj') || ev.fileName.endsWith('.props'))
			StatusBarController.update();
	}));

	return exports;
}

export function deactivate() {
	StateController.deactivate();
}
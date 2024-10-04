import { MonoDebugConfigurationProvider } from './providers/monoDebugConfigurationProvider';
import { DotNetTaskProvider } from './providers/dotnetTaskProvider';
import { ConfigurationController } from './controllers/configurationController';
import { StatusBarController } from './controllers/statusbarController';
import { InteropController } from './controllers/interopController';
import { StateController } from './controllers/stateController';
import { PublicExports } from './publicExports';
import { ModulesView } from './features/modulesView';
import { HotReload } from './features/hotReload';
import { XamlServer } from './features/xamlServer';
import * as res from './resources/constants';
import * as vscode from 'vscode';


export function activate(context: vscode.ExtensionContext): PublicExports | undefined {
	if (vscode.workspace.workspaceFolders === undefined) 
		return undefined;

	const exports = new PublicExports();
	
	InteropController.activate(context);
	ConfigurationController.activate(context);
	StateController.activate(context);
	StatusBarController.activate(context);
	StatusBarController.update();

	ModulesView.feature.activate(context);
	HotReload.feature.activate(context);
	XamlServer.feature.activate(context);
	
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveProject, StatusBarController.showQuickPickProject));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveConfiguration, StatusBarController.showQuickPickConfiguration));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveDevice, StatusBarController.showQuickPickDevice));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveTargetFramework, () => ConfigurationController.getTargetFramework()));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveConfiguration, () => ConfigurationController.configuration));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveProjectPath, () => ConfigurationController.project?.path));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveDeviceName, () => ConfigurationController.device?.name));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdActiveDeviceSerial, () => ConfigurationController.device?.serial));

	context.subscriptions.push(vscode.debug.registerDebugConfigurationProvider(res.debuggerMeteorId, new MonoDebugConfigurationProvider()));
	context.subscriptions.push(vscode.tasks.registerTaskProvider(res.taskDefinitionId, new DotNetTaskProvider()));

	return exports;
}

export function deactivate() {
	StateController.deactivate();
}
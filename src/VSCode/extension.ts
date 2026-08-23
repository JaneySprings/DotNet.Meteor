import { CoreClrConfigurationProvider } from './providers/coreClrConfigurationProvider';
import { DotNetTaskProvider } from './providers/dotnetTaskProvider';
import { ConfigurationController } from './controllers/configurationController';
import { StatusBarController } from './controllers/statusbarController';
import { Interop } from './interop/interop';
import { StateController } from './controllers/stateController';
import { MauiEssentials } from './features/mauiEssentials';
import { RemoteHostProvider } from './features/removeHostProvider';
import * as res from './resources/constants';
import * as vscode from 'vscode';


export function activate(context: vscode.ExtensionContext) {
	if (!Interop.initialize(context.extensionPath)) {
		vscode.window.showErrorMessage(res.messageInvalidDotnetSdk, { modal: true });
		return undefined;
	}

	if (vscode.workspace.workspaceFolders === undefined)
		return undefined;

	ConfigurationController.activate(context);
	StateController.activate(context);
	StatusBarController.activate(context);

	MauiEssentials.feature.activate(context);
	RemoteHostProvider.feature.activate(context);

	context.subscriptions.push(vscode.debug.registerDebugConfigurationProvider(res.debuggerMeteorId, new CoreClrConfigurationProvider()));
	context.subscriptions.push(vscode.tasks.registerTaskProvider(res.taskDefinitionId, new DotNetTaskProvider()));

	return exports;
}

export function deactivate() {
	StateController.deactivate();
}
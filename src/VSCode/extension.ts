import { CoreClrConfigurationProvider } from './providers/coreClrConfigurationProvider';
import { DotNetTaskProvider } from './providers/dotnetTaskProvider';
import { ConfigurationController } from './controllers/configurationController';
import { StatusBarController } from './controllers/statusbarController';
import { HotReloadController } from './controllers/hotreloadController';
import { StateController } from './controllers/stateController';
import { XamlController } from './controllers/xamlController';
import { Interop } from './interop/interop';
import * as res from './resources/constants';
import * as vscode from 'vscode';


export function activate(context: vscode.ExtensionContext) {
	if (!Interop.initialize(context.extensionPath)) {
		vscode.window.showErrorMessage(res.messageInvalidDotnetSdk, { modal: true });
		return undefined;
	}

	ConfigurationController.activate(context);
	StateController.activate(context);
	StatusBarController.activate(context);
	HotReloadController.activate(context);
	XamlController.activate(context);

	context.subscriptions.push(vscode.debug.registerDebugConfigurationProvider(res.debuggerMeteorId, new CoreClrConfigurationProvider()));
	context.subscriptions.push(vscode.tasks.registerTaskProvider(res.taskDefinitionId, new DotNetTaskProvider()));
}

export function deactivate() {
	StateController.deactivate();
}
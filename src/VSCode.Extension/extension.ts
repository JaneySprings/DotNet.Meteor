import { DotNetDebuggerConfiguration } from './tasks/debug';
import { SchemaController } from './xaml/schemacontroller';
import { DotNetTaskProvider } from './tasks/build';
import { XamlService } from './xaml/xamlservice';
import { UIController } from './controller';
import * as res from './resources';
import * as vscode from 'vscode';


export function activate(context: vscode.ExtensionContext) {
	if (vscode.workspace.workspaceFolders === undefined) 
		return;
	
	UIController.activate(context);
	XamlService.activate(context);
	UIController.update();

	/* Commands */
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveProject, UIController.showQuickPickProject));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveConfiguration, UIController.showQuickPickTarget));
	context.subscriptions.push(vscode.commands.registerCommand(res.commandIdSelectActiveDevice, UIController.showQuickPickDevice));
	/* Providers */
	context.subscriptions.push(vscode.debug.registerDebugConfigurationProvider(res.debuggerMeteorId, new DotNetDebuggerConfiguration()));
	context.subscriptions.push(vscode.tasks.registerTaskProvider(res.taskDefinitionId, new DotNetTaskProvider()));
	/* Events */
	context.subscriptions.push(vscode.debug.onDidStartDebugSession(() => vscode.commands.executeCommand(res.commandIdFocusOnDebug)));
	context.subscriptions.push(vscode.workspace.onDidChangeWorkspaceFolders(UIController.update));
	context.subscriptions.push(vscode.workspace.onDidSaveTextDocument(ev => {
		if (ev.fileName.endsWith('proj') || ev.fileName.endsWith('.props'))
			UIController.update();
	}));
	context.subscriptions.push(vscode.tasks.onDidEndTask(ev => {
		if (ev.execution.task.definition.type.includes(res.taskDefinitionId))
			SchemaController.invalidate();
	}));
}

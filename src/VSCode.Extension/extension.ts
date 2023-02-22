import { DotNetDebuggerConfiguration } from './tasks/debug';
import { SchemaController } from './xaml/schemacontroller';
import { DotNetTaskProvider } from './tasks/build';
import { XamlService } from './xaml/xamlservice';
import { Configuration } from './configuration';
import { UIController } from './controller';
import { CommandInterface } from './bridge';
import * as res from './resources';
import * as vscode from 'vscode';


export function activate(context: vscode.ExtensionContext) {
	if (vscode.workspace.workspaceFolders === undefined) 
		return;
	
	UIController.activate(context);
	XamlService.activate(context);

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
	context.subscriptions.push(vscode.workspace.onDidSaveTextDocument(ev => {
		if (ev.fileName.endsWith('.csproj') || ev.fileName.endsWith('.props'))
			analyzeWorkspace();
	}));
	context.subscriptions.push(vscode.tasks.onDidEndTask(ev => {
		if (ev.execution.task.definition.type.includes(res.taskDefinitionId))
			SchemaController.invalidate();
	}));
}


function analyzeWorkspace() {
	const folders = vscode.workspace.workspaceFolders!.map(it => it.uri.fsPath);
	CommandInterface.analyzeWorkspaceAsync(folders, items => {
		if (items.length === 0) {
			Configuration.project = undefined;
			UIController.hide();
			return;
		}

		UIController.projects = items;
		UIController.show();

		Configuration.project = items.find(it => it.path === Configuration.project?.path);
		UIController.performSelectProject(Configuration.project);
		UIController.performSelectTarget(Configuration.target);
	});
}

function analyzeDevices() {
	CommandInterface.devicesAsync(items => {
		if (items.length === 0) 
			return;

		UIController.devices = items;
		UIController.performSelectDevice();
	});
}
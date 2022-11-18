import * as vscode from 'vscode';
import { Configuration } from './configuration';
import * as constants from './constants';
import { DebuggerUtils } from "./bridge";
import { Interface } from './interface';
import { Target } from './configuration';


export function activate(context: vscode.ExtensionContext) {
	if (vscode.workspace.workspaceFolders === undefined) 
		return;

	fetchWorkspace();
	fetchDevices();
	
	if (Configuration.workspaceProjects.length === 0)
		return;
	
	Interface.initStatusItemCommands();
	Configuration.selectProject(Configuration.workspaceProjects[0]);
	Configuration.selectTarget(Target.Debug);
	Configuration.selectDevice(Configuration.mobileDevices[0]);

	context.subscriptions.push(vscode.commands.registerCommand(constants.commandSelectProjectIdentifier, () => {
		Interface.showQuickPickProject();
	}));
	context.subscriptions.push(vscode.commands.registerCommand(constants.commandSelectTargetIdentifier, () => {
		Interface.showQuickPickTarget();
	}));
	context.subscriptions.push(vscode.commands.registerCommand(constants.commandSelectDeviceIdentifier, () => {
		Interface.showQuickPickDevice();
	}));
}

export function deactivate() {}


function fetchWorkspace() {
	const workspacePath = vscode.workspace.workspaceFolders![0].uri.fsPath;
	Configuration.workspaceProjects = DebuggerUtils.findProjects(workspacePath);
}

function fetchDevices() {
	const androidDevices = DebuggerUtils.androidDevices();
	const appleDevices = DebuggerUtils.appleDevices();
	Configuration.mobileDevices = androidDevices.concat(appleDevices);
}
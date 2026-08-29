import { ConfigurationController } from "../controllers/configurationController";
import { ChildProcess, spawn } from "child_process";
import { Interop } from "../interop/interop";
import * as res from '../resources/constants';
import * as vscode from 'vscode';
import * as path from 'path';

export class HotReloadController {
    private static hotReloadEnabledKey: string = `${res.extensionId}.hotReloadEnabled`;
    private static reloadAgent: ChildProcess | undefined;

    public static async activate(context: vscode.ExtensionContext): Promise<void> {
        context.subscriptions.push(vscode.workspace.onWillSaveTextDocument(ev => {
            const extName = path.extname(ev.document.fileName);
            if (!ev.document.isDirty || extName !== '.xaml')
                return;

            if (ConfigurationController.getSetting<boolean>(res.configIdApplyHotReloadChangesOnSave, true))
                HotReloadController.sendAgentNotification(ev.document.fileName);
        }));
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdTriggerHotReload, () => {
            if (vscode.window.activeTextEditor === undefined)
                return;
            if (vscode.window.activeTextEditor.document.isDirty) {
                vscode.window.activeTextEditor.document.save();
                if (ConfigurationController.getSetting<boolean>(res.configIdApplyHotReloadChangesOnSave, true))
                    return;
            }
            HotReloadController.sendAgentNotification(vscode.window.activeTextEditor.document.fileName);
        }));
        context.subscriptions.push(vscode.debug.onDidStartDebugSession(ev => {
            if (ev.type === res.debuggerMeteorId)
                HotReloadController.startAgent();
        }));
        context.subscriptions.push(vscode.debug.onDidTerminateDebugSession(ev => {
            if (ev.type === res.debuggerMeteorId)
                HotReloadController.stopAgent();
        }));
    }

    private static startAgent() {
        if (HotReloadController.reloadAgent !== undefined)
            HotReloadController.stopAgent();

        HotReloadController.reloadAgent = spawn('dotnet', [
            Interop.workspaceToolPath, 'hotreload',
            '--host-pid', process.pid.toString(),
            '--port', ConfigurationController.getSetting<number>(res.configIdHotReloadHostPort, 9988).toString(),
            '--mode', 'universal'
        ]);
        vscode.commands.executeCommand('setContext', HotReloadController.hotReloadEnabledKey, true);
    }
    private static stopAgent() {
        if (HotReloadController.reloadAgent === undefined)
            return;

        HotReloadController.reloadAgent.kill();
        HotReloadController.reloadAgent = undefined;
        vscode.commands.executeCommand('setContext', HotReloadController.hotReloadEnabledKey, false);
    }
    private static sendAgentNotification(path: string) {
        if (HotReloadController.reloadAgent !== undefined && path.endsWith('.xaml'))
            HotReloadController.reloadAgent.stdin?.write(`${path}\n`);
    }
}
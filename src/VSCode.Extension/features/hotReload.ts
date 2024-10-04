import { ConfigurationController } from '../controllers/configurationController';
import { spawn, ChildProcess } from 'child_process';
import * as res from '../resources/constants';
import * as vscode from 'vscode';
import * as path from "path";

export class HotReload {
    public static feature : HotReload = new HotReload();
    private static toolPath: string;

    private isProgrammaticalySaving: boolean = false;
    private agent: ChildProcess | undefined;
    
    public activate(context: vscode.ExtensionContext) {
        const serverExecutable = path.join(context.extensionPath, "extension", "bin", "HotReload", "DotNet.Meteor.HotReload");
        const serverExtension = process.platform === 'win32' ? '.exe' : '';
        HotReload.toolPath = serverExecutable + serverExtension;

        context.subscriptions.push(vscode.workspace.onDidSaveTextDocument(ev => {
            if (this.isProgrammaticalySaving)
                return;
            if (ConfigurationController.getSetting<boolean>(res.configIdApplyHotReloadChangesOnSave, true))
                this.reloadDocumentChanges(ev.fileName);
        }));
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdTriggerHotReload, async () => {
            if (vscode.window.activeTextEditor !== undefined) {
                this.isProgrammaticalySaving = true;
                await vscode.window.activeTextEditor.document.save();
                this.isProgrammaticalySaving = false;

                this.reloadDocumentChanges(vscode.window.activeTextEditor.document.fileName);
            }
        }));
        context.subscriptions.push(vscode.debug.onDidStartDebugSession(ev => {
            if (ev.type === res.debuggerMeteorId || ev.type === res.debuggerVsdbgId)
                this.startAgent();
        }));
        context.subscriptions.push(vscode.debug.onDidTerminateDebugSession(ev => {
            if (ev.type === res.debuggerMeteorId || ev.type === res.debuggerVsdbgId)
                this.stopAgent();
        }));
    }

    private startAgent() {
        if (this.agent !== undefined)
            this.stopAgent();
        
        const args = [ process.pid.toString(), ConfigurationController.getReloadHostPort().toString(), 'universal'];
        this.agent = spawn(HotReload.toolPath, args);
    }
    private stopAgent() {
        if (this.agent === undefined)
            return;
        
        this.agent.kill();
        this.agent = undefined;
    }
    private reloadDocumentChanges(path: string) {
        if (this.agent === undefined || !path.endsWith('.xaml'))
            return;

        this.agent.stdin?.write(`${path}\n`);
    }
}
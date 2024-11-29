import { ConfigurationController } from "../controllers/configurationController";
import { LanguageClient, ServerOptions } from "vscode-languageclient/node";
import { ChildProcess, spawn } from "child_process";
import * as res from '../resources/constants';
import * as vscode from 'vscode';
import * as path from "path";

export class MauiEssentials {
    public static feature : MauiEssentials = new MauiEssentials();

    private static hotReloadEnabledKey: string = `${res.extensionId}.hotReloadEnabled`;

    private static languageServerPath: string;
    private lanuageServerClient: LanguageClient | undefined;
    
    private static reloadAgentPath: string;
    private reloadAgent: ChildProcess | undefined;


    public async activate(context: vscode.ExtensionContext): Promise<void> {
        // Deactivate if no XAML files are found
        if ((await vscode.workspace.findFiles('**/*.xaml')).length <= 0)
            return;

        // Hot Reload
        const agentExecutable = path.join(context.extensionPath, "extension", "bin", "HotReload", "DotNet.Meteor.HotReload");
        const agentExtension = ConfigurationController.onWindows ? '.exe' : '';
        MauiEssentials.reloadAgentPath = agentExecutable + agentExtension;

        let isProgrammaticalySaving = false;
        context.subscriptions.push(vscode.workspace.onDidSaveTextDocument(ev => {
            if (isProgrammaticalySaving)
                return;
            if (ConfigurationController.getSetting<boolean>(res.configIdApplyHotReloadChangesOnSave, true))
                this.reloadDocumentChanges(ev.fileName);
        }));
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdTriggerHotReload, async () => {
            if (vscode.window.activeTextEditor !== undefined) {
                isProgrammaticalySaving = true;
                await vscode.window.activeTextEditor.document.save();
                isProgrammaticalySaving = false;

                this.reloadDocumentChanges(vscode.window.activeTextEditor.document.fileName);
            }
        }));
        context.subscriptions.push(vscode.debug.onDidStartDebugSession(ev => {
            if ((ev.type === res.debuggerMeteorId || ev.type === res.debuggerVsdbgId))
                MauiEssentials.feature.startAgent();
        }));
        context.subscriptions.push(vscode.debug.onDidTerminateDebugSession(ev => {
            if (ev.type === res.debuggerMeteorId || ev.type === res.debuggerVsdbgId)
                MauiEssentials.feature.stopAgent();
        }));
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdXamlReplaceCode, async (edit) => {
            const newEdit = new vscode.WorkspaceEdit();
            const uri = vscode.Uri.parse(edit.TextDocument.Uri);
            for (let i = 0; i < edit.Edits.length; i++) {
                const start = new vscode.Position(edit.Edits[i].range.Start.Line, edit.Edits[i].range.Start.Character);
                const end = new vscode.Position(edit.Edits[i].range.End.Line, edit.Edits[i].range.End.Character);
                const range = new vscode.Range(start, end);
                newEdit.replace(uri, range, edit.Edits[i].newText);
            }
            await vscode.workspace.applyEdit(newEdit);
            vscode.workspace.textDocuments.forEach(async doc => {
                if (doc.uri.fsPath === uri.fsPath)
                    await doc.save();
            });
        }));
        
        // Language Server
        const serverExecutable = path.join(context.extensionPath, "extension", "bin", "Xaml", "DotNet.Meteor.Xaml.LanguageServer");
        const serverExtension = ConfigurationController.onWindows ? '.exe' : '';
        MauiEssentials.languageServerPath = serverExecutable + serverExtension;

        context.subscriptions.push(vscode.tasks.onDidEndTaskProcess(ev => {
            if (ev.execution.task.definition.type.includes(res.taskDefinitionId) && ev.exitCode === 0)
                MauiEssentials.feature.restartServer();
        }));
        
        MauiEssentials.feature.startServer();
        if (MauiEssentials.feature.lanuageServerClient !== undefined)
            context.subscriptions.push(MauiEssentials.feature.lanuageServerClient);
    }

    public startServer() {
        const serverArguments: string[] = [ /*ConfigurationController.project?.path ?? ""*/ ]; //TODO: Wait for initialization
        const serverOptions: ServerOptions = { command: MauiEssentials.languageServerPath, args: serverArguments };
        MauiEssentials.feature.lanuageServerClient = new LanguageClient(res.extensionId, res.extensionId, serverOptions, {
            diagnosticCollectionName: res.extensionDisplayName,
            synchronize: {
                configurationSection: res.extensionId,
            },
            connectionOptions: {
                maxRestartCount: 2,
            }
        });
        MauiEssentials.feature.lanuageServerClient?.start();
    }
    public stopServer() {
        MauiEssentials.feature.lanuageServerClient?.stop();
        MauiEssentials.feature.lanuageServerClient?.dispose();
    }
    public restartServer() {
        MauiEssentials.feature.stopServer();
        MauiEssentials.feature.startServer();
    }

    private startAgent() {
        if (MauiEssentials.feature.reloadAgent !== undefined)
            MauiEssentials.feature.stopAgent();
        
        const args = [ process.pid.toString(), ConfigurationController.getReloadHostPort().toString(), 'universal'];
        MauiEssentials.feature.reloadAgent = spawn(MauiEssentials.reloadAgentPath, args);
        vscode.commands.executeCommand('setContext', MauiEssentials.hotReloadEnabledKey, true);
    }
    private stopAgent() {
        if (MauiEssentials.feature.reloadAgent === undefined)
            return;
        
        MauiEssentials.feature.reloadAgent.kill();
        MauiEssentials.feature.reloadAgent = undefined;
        vscode.commands.executeCommand('setContext', MauiEssentials.hotReloadEnabledKey, false);
    }
    private reloadDocumentChanges(path: string) {
        if (MauiEssentials.feature.reloadAgent !== undefined && path.endsWith('.xaml'))
            MauiEssentials.feature.reloadAgent.stdin?.write(`${path}\n`);
    }
}
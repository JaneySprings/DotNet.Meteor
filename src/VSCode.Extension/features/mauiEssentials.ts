import { ConfigurationController } from "../controllers/configurationController";
import { LanguageClient, ServerOptions } from "vscode-languageclient/node";
import { ChildProcess, spawn } from "child_process";
import * as res from '../resources/constants';
import * as vscode from 'vscode';
import * as path from "path";

export class MauiEssentials {
    public static feature : MauiEssentials = new MauiEssentials();

    private static lanuageServerClient: LanguageClient;
    private static languageServerPath: string;
    private static reloadAgentPath: string;

    private isProgrammaticalySaving: boolean = false;
    private reloadAgent: ChildProcess | undefined;

    public async activate(context: vscode.ExtensionContext): Promise<void> {
        const agentExecutable = path.join(context.extensionPath, "extension", "bin", "HotReload", "DotNet.Meteor.HotReload");
        const agentExtension = process.platform === 'win32' ? '.exe' : '';
        MauiEssentials.reloadAgentPath = agentExecutable + agentExtension;
        
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

        if ((await vscode.workspace.findFiles('**/*.xaml')).length > 0)
            this.activateServer(context);
    }

    private activateServer(context: vscode.ExtensionContext) {
        const extensionPath = context.extensionPath;
        const serverExecutable = path.join(extensionPath, "extension", "bin", "Xaml", "DotNet.Meteor.Xaml.LanguageServer");
        const serverExtension = process.platform === 'win32' ? '.exe' : '';
        MauiEssentials.languageServerPath = serverExecutable + serverExtension;
        
        this.startServer();

        context.subscriptions.push(MauiEssentials.lanuageServerClient);
        context.subscriptions.push(vscode.tasks.onDidEndTaskProcess(ev => {
            if (ev.execution.task.definition.type.includes(res.taskDefinitionId) && ev.exitCode === 0)
                this.restartServer();
        }));
    }
    private initialize() {
        const serverArguments: string[] = [ /*ConfigurationController.project?.path ?? ""*/ ]; //TODO: Wait for initialization
        const serverOptions: ServerOptions = { command: MauiEssentials.languageServerPath, args: serverArguments };
        MauiEssentials.lanuageServerClient = new LanguageClient(res.extensionId, res.extensionId, serverOptions, {
            diagnosticCollectionName: res.extensionDisplayName,
            synchronize: {
                configurationSection: res.extensionId,
            },
            connectionOptions: {
                maxRestartCount: 2,
            }
        });
    }
    public startServer() {
        this.initialize();
        MauiEssentials.lanuageServerClient.start();
    }
    public stopServer() {
        MauiEssentials.lanuageServerClient.stop();
        MauiEssentials.lanuageServerClient.dispose();
    }
    public restartServer() {
        this.stopServer();
        this.startServer();
    }

    private startAgent() {
        if (this.reloadAgent !== undefined)
            this.stopAgent();
        
        const args = [ process.pid.toString(), ConfigurationController.getReloadHostPort().toString(), 'universal'];
        this.reloadAgent = spawn(MauiEssentials.reloadAgentPath, args);
    }
    private stopAgent() {
        if (this.reloadAgent === undefined)
            return;
        
        this.reloadAgent.kill();
        this.reloadAgent = undefined;
    }
    private reloadDocumentChanges(path: string) {
        if (this.reloadAgent === undefined || !path.endsWith('.xaml'))
            return;

        this.reloadAgent.stdin?.write(`${path}\n`);
    }
}
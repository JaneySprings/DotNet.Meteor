import { LanguageClient, ServerOptions } from "vscode-languageclient/node";
import * as res from '../resources/constants';
import * as vscode from 'vscode';
import * as path from "path";

export class XamlServer {
    public static feature : XamlServer = new XamlServer();

    private static client: LanguageClient;
    private static command: string;

    public async activate(context: vscode.ExtensionContext): Promise<void> {
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

        if ((await vscode.workspace.findFiles('**/*.xaml')).length > 0)
            this.activateServer(context);
    }

    private activateServer(context: vscode.ExtensionContext) {
        const extensionPath = context.extensionPath;
        const serverExecutable = path.join(extensionPath, "extension", "bin", "Xaml", "DotNet.Meteor.Xaml.LanguageServer");
        const serverExtension = process.platform === 'win32' ? '.exe' : '';
        XamlServer.command = serverExecutable + serverExtension;
        
        this.start();

        context.subscriptions.push(XamlServer.client);
        context.subscriptions.push(vscode.tasks.onDidEndTaskProcess(ev => {
            if (ev.execution.task.definition.type.includes(res.taskDefinitionId) && ev.exitCode === 0)
                this.restart();
        }));
    }
    private initialize() {
        const serverArguments: string[] = [ /*ConfigurationController.project?.path ?? ""*/ ]; //TODO: Wait for initialization
        const serverOptions: ServerOptions = { command: XamlServer.command, args: serverArguments };
        XamlServer.client = new LanguageClient(res.extensionId, res.extensionId, serverOptions, {
            diagnosticCollectionName: res.extensionDisplayName,
            synchronize: {
                configurationSection: res.extensionId,
            },
            connectionOptions: {
                maxRestartCount: 2,
            }
        });
    }

    public start() {
        this.initialize();
        XamlServer.client.start();
    }
    public stop() {
        XamlServer.client.stop();
        XamlServer.client.dispose();
    }
    public restart() {
        this.stop();
        this.start();
    }
}
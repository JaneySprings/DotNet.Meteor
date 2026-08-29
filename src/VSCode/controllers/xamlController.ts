import { LanguageClient, ServerOptions } from "vscode-languageclient/node";
import { Interop } from "../interop/interop";
import * as res from '../resources/constants';
import * as vscode from 'vscode';

export class XamlController {
    private static lanuageServerClient: LanguageClient | undefined;

    public static async activate(context: vscode.ExtensionContext): Promise<void> {
        if ((await vscode.workspace.findFiles('**/*.xaml')).length <= 0)
            return;

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
        context.subscriptions.push(vscode.tasks.onDidEndTaskProcess(ev => {
            if (ev.execution.task.definition.type.includes(res.taskDefinitionId) && ev.exitCode === 0)
                XamlController.restartServer();
        }));

        XamlController.startServer();
    }

    private static startServer() {
        const serverOptions: ServerOptions = { command: 'dotnet', args: [Interop.xamlServerPath] };
        XamlController.lanuageServerClient = new LanguageClient(res.extensionId, res.extensionId, serverOptions, {
            diagnosticCollectionName: res.extensionDisplayName,
            synchronize: {
                configurationSection: res.extensionId,
            },
            connectionOptions: {
                maxRestartCount: 2,
            }
        });
        XamlController.lanuageServerClient?.start();
    }
    private static stopServer() {
        XamlController.lanuageServerClient?.stop();
        XamlController.lanuageServerClient?.dispose();
    }
    private static restartServer() {
        XamlController.stopServer();
        XamlController.startServer();
    }
}
import { LanguageClient, ServerOptions } from "vscode-languageclient/node";
import { ConfigurationController } from "./configurationController";
import * as res from './resources/constants';
import * as vscode from 'vscode';
import * as path from "path";

export class XamlController {
    private static client: LanguageClient;
    private static command: string;

    public static activate(context: vscode.ExtensionContext) {
        context.subscriptions.push(vscode.workspace.onDidSaveTextDocument(ev => {
            if (ConfigurationController.getSetting<boolean>(res.configIdApplyHotReloadChangesOnSave, true))
                XamlController.reloadDocumentChanges(ev.fileName);
        }));
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdTriggerHotReload, async () => {
            if (vscode.window.activeTextEditor !== undefined) {
                await vscode.window.activeTextEditor.document.save();
                XamlController.reloadDocumentChanges(vscode.window.activeTextEditor.document.fileName);
            }
        }));
        context.subscriptions.push(vscode.tasks.onDidEndTask(ev => {
            if (ev.execution.task.definition.type.includes(res.taskDefinitionId))
                XamlController.restart();
        }));
    
        const extensionPath = context.extensionPath;
        const serverExecutable = path.join(extensionPath, "extension", "bin", "Xaml", "DotNet.Meteor.Xaml.LanguageServer");
        const serverExtension = process.platform === 'win32' ? '.exe' : '';
        XamlController.command = serverExecutable + serverExtension;
        XamlController.start();
    }

    private static initialize() {
        const serverOptions: ServerOptions = { command: XamlController.command };
        XamlController.client = new LanguageClient(res.extensionId, res.extensionId, serverOptions, { 
            diagnosticCollectionName: "Xaml",
            synchronize: { 
                configurationSection: res.extensionId,
            },
            connectionOptions: {
                maxRestartCount: 2,
            }
        });
    }
    public static start() {
        XamlController.initialize();
        XamlController.client.start();
    }
    public static stop() {
        XamlController.client.stop();
        XamlController.client.dispose();
    }
    public static restart() {
        XamlController.stop();
        XamlController.start();
    }

    public static reloadDocumentChanges(filePath: string) {
        if (filePath.endsWith('.xaml') && vscode.debug.activeDebugSession?.configuration.type === res.debuggerMeteorId)
            vscode.debug.activeDebugSession.customRequest('hotReload', { filePath: filePath });
    }
}
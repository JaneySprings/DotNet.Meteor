import { XamlCompletionItemProvider } from './features/xamlCompletionItemProvider';
import { ConfigurationController } from "../configurationController";
import { XamlCommandController } from './xamlCommandController';
import * as res from '../resources/constants';
import * as vscode from 'vscode';
import * as path from "path";
import * as fs from "fs";

export class XamlController {
    private static xamlSchemaAliases: any[] = [];
    private static extensionVersion: string;

    public static activate(context: vscode.ExtensionContext) {
        /* Hot Reload */
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
        /* Xaml IntelliSense */
        context.subscriptions.push(vscode.languages.registerCompletionItemProvider({ pattern: '**/*.xaml' }, new XamlCompletionItemProvider(), ':', '.', '<', ' ',));
        context.subscriptions.push(vscode.tasks.onDidEndTask(ev => {
            if (ev.execution.task.definition.type.includes(res.taskDefinitionId))
                XamlController.regenerate();
        }));

        XamlController.extensionVersion = context.extension.packageJSON.version;
        XamlCommandController.activate(context);
    }

    public static reloadDocumentChanges(filePath: string) {
        if (filePath.endsWith('.xaml') && vscode.debug.activeDebugSession?.configuration.type === res.debuggerMeteorId)
            vscode.debug.activeDebugSession.customRequest('hotReload', { filePath: filePath });
    }

    public static getTypes(definition: string | undefined): any[] { 
        if (definition === undefined)
            return [];

        if (definition.startsWith("clr-namespace:")) {
            if (definition.includes(";assembly="))
                definition = definition.split(";assembly=")[1];
            else 
                definition = definition.replace("clr-namespace:", "");
            
            const schema = this.xamlSchemaAliases.find(x => definition === x.assembly);
            if (schema !== undefined) 
                return schema.types;
        } else {
            const schemas = this.xamlSchemaAliases.filter(x => definition === x.xmlns);
            const types = [];
            for (const schema of schemas) 
                types.push(...schema.types);
            
            return types;
        }

        return [];
    }
    public static async generate() {
        if (this.xamlSchemaAliases.length > 0) 
            return;

        const projectPath = ConfigurationController.project?.path;
        if (projectPath === undefined)
            return;

        await XamlCommandController.xamlSchema(projectPath);
        const generatedPath = path.join(path.dirname(projectPath), '.meteor', 'generated');
        if (fs.existsSync(generatedPath) === false)
            return;

        const files = await fs.promises.readdir(generatedPath);
        for (const file of files) {
            const filePath = path.join(generatedPath, file);
            const alias = JSON.parse(fs.readFileSync(filePath).toString());
            if (!alias.version || (alias.version && alias.version !== XamlController.extensionVersion))
                continue;

            this.xamlSchemaAliases.push(alias);
        }
    }
    public static regenerate() {
        this.xamlSchemaAliases = [];
        XamlController.generate();
    }
}
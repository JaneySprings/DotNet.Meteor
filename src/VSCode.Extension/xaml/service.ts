import { XamlCompletionItemProvider } from './completions';
import { ConfigurationController } from "../configuration";
import { XamlLinterProvider } from './linter';
import { CommandController } from "../bridge";
import * as res from '../resources';
import * as vscode from 'vscode';
import * as path from "path";
import * as fs from "fs";


export const languageId = 'xml';

export class XamlController {
    private static xamlSchemaAliases: any[] = [];
    private static extensionVersion: string;

    public static activate(context: vscode.ExtensionContext) {
        const schemaSelector = { language: languageId, scheme: 'file' };
        XamlController.extensionVersion = context.extension.packageJSON.version;

        context.subscriptions.push(new XamlLinterProvider(context));
        context.subscriptions.push(vscode.languages.registerCompletionItemProvider(
            schemaSelector, new XamlCompletionItemProvider(), ':', '.', '<', ' ',
        ));
        context.subscriptions.push(vscode.workspace.onDidSaveTextDocument(ev => {
            if (ev.fileName.endsWith('.xaml') && vscode.debug.activeDebugSession?.configuration.type === res.debuggerMeteorId)
                CommandController.xamlReload(ConfigurationController.getReloadHostPort(), ev.fileName);
        }));

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

        await CommandController.xamlSchema(projectPath);
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
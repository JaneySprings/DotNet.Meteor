import { XamlCompletionItemProvider } from './completions';
import { Configuration } from "../configuration";
import { XamlLinterProvider } from './linter';
import { CommandInterface } from "../bridge";
import * as res from '../resources';
import * as vscode from 'vscode';
import * as path from "path";
import * as fs from "fs";


export const languageId = 'xml';

export class XamlController {
    private static xamlSchemaAliases: any[] = [];

    public static activate(context: vscode.ExtensionContext) {
        const schemaSelector = { language: languageId, scheme: 'file' };

        context.subscriptions.push(new XamlLinterProvider(context));
        context.subscriptions.push(vscode.languages.registerCompletionItemProvider(
            schemaSelector, new XamlCompletionItemProvider(), ':', '.', '<', ' ',
        ));
        context.subscriptions.push(vscode.workspace.onDidSaveTextDocument(ev => {
            if (ev.fileName.endsWith('.xaml') && vscode.debug.activeDebugSession?.configuration.type === res.debuggerMeteorId)
                CommandInterface.xamlReload(XamlController.getReloadHostPort(), ev.fileName);
        }));

    }

    public static getReloadHostPort(): number {
        return Configuration.getSetting<number>(
            res.configIdHotReloadHostPort, 
            res.configDefaultotReloadHostPort
        );
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

        const projectPath = Configuration.project?.path;
        if (projectPath === undefined)
            return;

        await CommandInterface.xamlSchema(projectPath);

        const generatedPath = path.join(path.dirname(projectPath), '.meteor', 'generated');
        if (fs.existsSync(generatedPath) === false)
            return;

        const files = await fs.promises.readdir(generatedPath);
        for (const file of files) {
            const filePath = path.join(generatedPath, file);
            const dataArray = JSON.parse(fs.readFileSync(filePath).toString());
            this.xamlSchemaAliases.push(dataArray);
        }
    }
    public static regenerate() {
        this.xamlSchemaAliases = [];
        XamlController.generate();
    }
}
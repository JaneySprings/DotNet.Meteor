import { XamlCompletionItemProvider } from './completions';
import { Configuration } from "../configuration";
import { XamlLinterProvider } from './linter';
import { CommandInterface } from "../bridge";
import * as vscode from 'vscode';
import * as path from "path";
import * as fs from "fs";


export const languageId = 'xml';

export class XamlService {
    private static xamlSchemaAliases: any[] = [];

    public static activate(context: vscode.ExtensionContext) {
        const schemaSelector = { language: languageId, scheme: 'file' };
    
        context.subscriptions.push(new XamlLinterProvider(context));
        context.subscriptions.push(vscode.languages.registerCompletionItemProvider(
            schemaSelector, new XamlCompletionItemProvider(), ':', '.', '<', ' ',
        ));
        
    }


    public static getTypes(namespace: string | undefined): any[] { 
        if (namespace === undefined)
            return [];

        const schema = this.xamlSchemaAliases.find(x => namespace.includes(x.xmlns));
        if (schema === undefined) 
            return [];

        return schema.types;
    }
    public static getAttachedTypes(namespace: string | undefined): any[] {
        const result: any[] = [];
        if (namespace === undefined)
            return result;

        const schema = this.xamlSchemaAliases.find(x => namespace.includes(x.xmlns));
        for (const type of schema.types) {
            if (type.attributes.find((x: any) => x.isAttached) !== undefined) {
                result.push(type);
            }
        }
        return result;
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
        XamlService.generate();
    }
}
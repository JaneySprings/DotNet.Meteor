import * as vscode from 'vscode';
import { XamlCompletionItemProvider } from './completionprovider';
import { XamlSchemaPropertiesArray } from './types';
import XamlLinterProvider from './linterprovider';
import { CommandInterface } from '../bridge';
import { Configuration } from '../configuration';


export const languageId = 'xml';

export class XamlService {
    public static activate(context: vscode.ExtensionContext) {
        const schemaPropertiesArray = new XamlSchemaPropertiesArray();
        const schemaSelector = { language: languageId, scheme: 'file' };
    
        context.subscriptions.push(vscode.languages.registerCompletionItemProvider(schemaSelector, new XamlCompletionItemProvider()));
        context.subscriptions.push(new XamlLinterProvider(context, schemaPropertiesArray));
        
        this.generateSchema();
    }

    public static generateSchema() {
        const framework = Configuration.project!.frameworks?.find(it => it.includes(Configuration.device!.platform!))
        const path = Configuration.project!.path ?? '';
        const rid = Configuration.device!.runtime_id ?? '';
        CommandInterface.xamlSchema(path, framework!, rid, (succeeded: boolean) => {
            if (!succeeded) return;


        });
    }
}
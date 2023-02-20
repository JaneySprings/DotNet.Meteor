import * as vscode from 'vscode';
import { XamlCompletionItemProvider } from './completionprovider';
import { XamlSchemaPropertiesArray } from './types';
import XamlLinterProvider from './linterprovider';


export const languageId = 'xml';

export class XamlService {
    public static activate(context: vscode.ExtensionContext) {
        const schemaPropertiesArray = new XamlSchemaPropertiesArray();
        const schemaSelector = { language: languageId, scheme: 'file' };
    
        context.subscriptions.push(vscode.languages.registerCompletionItemProvider(schemaSelector, new XamlCompletionItemProvider()));
        context.subscriptions.push(new XamlLinterProvider(context, schemaPropertiesArray));
    }
}
import * as vscode from 'vscode';
import { XamlSchemaPropertiesArray } from './types';
import XamlLinterProvider from './linterprovider';
import XamlCompletionItemProvider from './completionprovider';


export const languageId = 'xml';

export function activate (context: vscode.ExtensionContext) {
    const schemaPropertiesArray = new XamlSchemaPropertiesArray();
    const schemaSelector = { language: languageId, scheme: 'file' };

    context.subscriptions.push(vscode.languages.registerCompletionItemProvider(schemaSelector, new XamlCompletionItemProvider(context, schemaPropertiesArray)));
    context.subscriptions.push(new XamlLinterProvider(context, schemaPropertiesArray));
}

/* eslint-disable no-return-assign */
import * as vscode from 'vscode';
import { SchemaController } from './schemacontroller';
import { languageId } from './xamlservice';
import XamlParser from './xamlparser';


export class XamlCompletionItemProvider implements vscode.CompletionItemProvider {
    async provideCompletionItems (textDocument: vscode.TextDocument, position: vscode.Position, token: vscode.CancellationToken, _context: vscode.CompletionContext): Promise<vscode.CompletionItem[] | vscode.CompletionList> {
        await SchemaController.prepareXamlSchemaAliases();

        const documentContent = textDocument.getText();
        const offset = textDocument.offsetAt(position);
        const scope = await XamlParser.getScopeForPosition(documentContent, offset);
        let completionItems: vscode.CompletionItem[] = [];

        if (textDocument.languageId === languageId && textDocument.fileName.includes(".xaml")) {
            if (scope.tagName === undefined)
                return [];
            // Element
            if (scope.context === "element" && !scope.tagName.includes(".")) {
                const xmlns = XamlParser.getXmlnsForPrefix(documentContent, scope.tagPrefix);
                const types = SchemaController.xamlAliasByNamespace(xmlns);

                for (var i = 0; i < types.length; i++) {
                    const ci = new vscode.CompletionItem(types[i].name, vscode.CompletionItemKind.Class);
                    ci.detail = `Class ${types[i].namespace}.${types[i].name}`;
                    ci.documentation = types[i].doc;
                    completionItems.push(ci);
                }
            // Attribute
            } else if (scope.context === "attribute") {
                const xmlns = XamlParser.getXmlnsForPrefix(documentContent, scope.tagPrefix);
                const types = SchemaController.xamlAliasByNamespace(xmlns);
                const findTag = types.find(t => t.name === scope.tagName);

                if (findTag !== undefined) {
                    for (let i = 0; i < findTag.attributes.length; i++) {
                        const ci = new vscode.CompletionItem(findTag.attributes[i].name);
                        ci.documentation = findTag.attributes[i].doc;
                        if ((typeof(findTag.attributes[i].type) === 'string' && (findTag.attributes[i].type as string).includes("Event"))) {
                            ci.detail = `Event ${findTag.attributes[i].namespace}.${findTag.attributes[i].name}`;
                            ci.kind = vscode.CompletionItemKind.Event;
                        } else {
                            ci.detail = `Property ${findTag.attributes[i].namespace}.${findTag.attributes[i].name}`;
                            ci.kind = vscode.CompletionItemKind.Property;
                        }
                        completionItems.push(ci);
                    }
                }
            // Value
            } else if (scope.context !== undefined) {
                const xmlns = XamlParser.getXmlnsForPrefix(documentContent, scope.tagPrefix);
                const types = SchemaController.xamlAliasByNamespace(xmlns);
                const findTag = types.find(t => t.name === scope.tagName);
        
                if (findTag !== undefined) {
                    const findProp = findTag.attributes.find((t: { name: any; }) => t.name === scope.tagAttribute);
        
                    if (findProp !== undefined) {
                        if (Array.isArray(findProp.type)) {
                            for (let i = 0; i < findProp.type.length; i++) {
                                const ci = new vscode.CompletionItem(findProp.type[i], vscode.CompletionItemKind.Enum);
                                completionItems.push(ci);
                            }
                        }
                    }
                }
            }
        }

        return completionItems;
    }
}

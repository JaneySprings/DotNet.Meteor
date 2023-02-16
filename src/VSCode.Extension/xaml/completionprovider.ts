/* eslint-disable no-return-assign */
import * as vscode from 'vscode';
import { CompletionString } from './types';
import { languageId } from './xamlservice';
import { SchemaController } from './schemacontroller';
import XamlParser from './xamlparser';


export class XamlCompletionItemProvider implements vscode.CompletionItemProvider {
    async provideCompletionItems (textDocument: vscode.TextDocument, position: vscode.Position, token: vscode.CancellationToken, _context: vscode.CompletionContext): Promise<vscode.CompletionItem[] | vscode.CompletionList> {
        SchemaController.generateXamlSchemaAliases();

        const documentContent = textDocument.getText();
        const offset = textDocument.offsetAt(position);
        const scope = await XamlParser.getScopeForPosition(documentContent, offset);
        let resultTexts: CompletionString[] = [];

        const controls = SchemaController.xamlAliasByName(scope.tagPrefix);

        // only for xaml
        if (textDocument.languageId === languageId && textDocument.fileName.includes(".xaml")) {
            if (token.isCancellationRequested) {
                resultTexts = [];
            } else if (scope.context === "text") {
                resultTexts = [];
            } else if (scope.tagName === undefined) {
                resultTexts = [];
            } else if (scope.context === "element" && !scope.tagName.includes(".")) {
                resultTexts = [];

                for (var i = 0; i < controls.length; i++) {
                    resultTexts.push(new CompletionString(controls[i].name));
                }

                resultTexts.map(t => t.type = vscode.CompletionItemKind.Class);
            } else if (scope.context === "attribute") {
                resultTexts = [];
                const findTag = controls.find(t => t.name === scope.tagName);

                if (findTag !== undefined) {
                    for (let i = 0; i < findTag.attributes.length; i++) {
                        const attr = new CompletionString(findTag.attributes[i].name);

                        if (typeof (findTag.attributes[i].type) === 'string' &&
                            (findTag.attributes[i].type as string).includes("Event")
                        ) {
                            attr.type = vscode.CompletionItemKind.Event;
                        } else {
                            attr.type = vscode.CompletionItemKind.Property;
                        }

                        resultTexts.push(attr);
                    }
                }

                // value?
            } else if (scope.context !== undefined) {
                resultTexts = [];

                const findTag = controls.find(t => t.name === scope.tagName);
                if (findTag !== undefined) {
                    const findProp = findTag.attributes
                        .find((t: { name: any; }) => t.name === scope.tagAttribute);

                    if (findProp !== undefined) {
                        if (Array.isArray(findProp.type)) {
                            for (let i = 0; i < findProp.type.length; i++) {
                                var enom = findProp.type[i];
                                const enomTyped = new CompletionString(enom);
                                enomTyped.type = vscode.CompletionItemKind.Enum;
                                resultTexts.push(enomTyped);
                            }
                        } else {
                            const normalTyped = new CompletionString(JSON.stringify(findProp));
                            normalTyped.type = vscode.CompletionItemKind.Text;
                            resultTexts.push(normalTyped);
                        }
                    }
                }
            } else {
                resultTexts = [];
            }
        }

        resultTexts = resultTexts.filter((v, i, a) => a.findIndex(e => e.name === v.name && e.comment === v.comment) === i);

        return resultTexts
            .map(t => {
                const ci = new vscode.CompletionItem(t.name, t.type);
                ci.detail = scope.context;
                ci.documentation = t.comment;

                if (t.type === vscode.CompletionItemKind.Property || t.type === vscode.CompletionItemKind.Event)
                    ci.insertText = `${t.name}=`;

                return ci;
            });
    }
}

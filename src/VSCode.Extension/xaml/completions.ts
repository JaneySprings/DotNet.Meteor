import { XamlController, languageId  } from './service';
import { XamlContext, XamlScope } from './types';
import { ContextService } from './context';
import * as vscode from 'vscode';


export class XamlCompletionItemProvider implements vscode.CompletionItemProvider {
    async provideCompletionItems(textDocument: vscode.TextDocument, position: vscode.Position, token: vscode.CancellationToken, _context: vscode.CompletionContext): Promise<vscode.CompletionItem[] | vscode.CompletionList> {
        await XamlController.generate();

        const documentContent = textDocument.getText();
        const offset = textDocument.offsetAt(position);
        const context = ContextService.getContext(documentContent, offset);
        if (textDocument.languageId === languageId && textDocument.fileName.includes(".xaml") && context) {
            if (context.scope === XamlScope.Tag) 
                return this.getClassCompletionItems(context);

            if (context.scope === XamlScope.Attribute || context.scope === XamlScope.Multiline)
                return this.getAttributeCompletionItems(context);

            if (context.scope === XamlScope.Static)
                return this.getStaticCompletionItems(context);

            if (context.scope === XamlScope.Value)
                return this.getValueCompletionItems(context);
        }
 
        return [];
    }

    private getClassCompletionItems(context: XamlContext): vscode.CompletionItem[] {
        const items: vscode.CompletionItem[] = []; 
        const types = XamlController.getTypes(context.tagContext?.namespace);
        for (var i = 0; i < types.length; i++) {
            const ci = new vscode.CompletionItem(types[i].name, vscode.CompletionItemKind.Class);
            ci.detail = `${types[i].namespace}.${types[i].name}`;
            items.push(ci);
        }

        if (!context.tagContext?.prefix) {
            for(const key in context.imports) {
                if (key !== '') {
                    const ci = new vscode.CompletionItem(key, vscode.CompletionItemKind.Module);
                    ci.detail = context.imports[key];
                    items.push(ci);

                    const prefixTypes = XamlController.getTypes(context.imports[key]);
                    for (var i = 0; i < prefixTypes.length; i++) {
                        const ci = new vscode.CompletionItem(prefixTypes[i].name, vscode.CompletionItemKind.Class);
                        ci.detail = `${prefixTypes[i].namespace}.${prefixTypes[i].name}`;
                        ci.insertText = `${key}:${prefixTypes[i].name}`;
                        items.push(ci);
                    }
                }
            }
        }  
        return items;
    }
    private getAttributeCompletionItems(context: XamlContext): vscode.CompletionItem[] {
        const items: vscode.CompletionItem[] = [];
        const types = XamlController.getTypes(context.attributeContext?.parent?.namespace);
        const findTag = types.find(t => t.name === context.attributeContext?.parent?.name);
        if (findTag !== undefined) {
            for (let i = 0; i < findTag.attributes.length; i++) {
                if (!findTag.attributes[i].isAttached) {
                    const ci = new vscode.CompletionItem(findTag.attributes[i].name);
                    ci.detail = `${findTag.attributes[i].namespace}.${findTag.attributes[i].name}`;
                    ci.kind = vscode.CompletionItemKind.Property;

                    if (context.scope === XamlScope.Attribute)
                        ci.insertText = new vscode.SnippetString(`${findTag.attributes[i].name}="$1"`);
                    if (findTag.attributes[i].isEvent) {
                        ci.kind = vscode.CompletionItemKind.Event;
                        if (context.scope === XamlScope.Multiline)
                            continue;
                    }

                    items.push(ci);
                }
            }
        }
        if (context.scope === XamlScope.Attribute) {
            const staticTypes = XamlController
                .getTypes(context.attributeContext?.namespace)
                .filter(t => t.attributes.find((a: any) => a.isAttached));
            for (let i = 0; i < staticTypes.length; i++) {
                const ci = new vscode.CompletionItem(staticTypes[i].name, vscode.CompletionItemKind.Module);
                ci.detail = `${staticTypes[i].namespace}.${staticTypes[i].name}`;
                ci.insertText = new vscode.SnippetString(`${staticTypes[i].name}.$1`);
                items.push(ci);
            }
        }
        return items;
    }
    private getStaticCompletionItems(context: XamlContext): vscode.CompletionItem[] {
        const items: vscode.CompletionItem[] = [];
        const types = XamlController.getTypes(context.attributeContext?.parent?.namespace);
        const findTag = types.find(t => t.name === context.attributeContext?.parent?.name);
        if (findTag !== undefined) {
            for (let i = 0; i < findTag.attributes.length; i++) {
                if (findTag.attributes[i].isAttached) {
                    const ci = new vscode.CompletionItem(findTag.attributes[i].name);
                    ci.detail = `${findTag.attributes[i].namespace}.${findTag.attributes[i].name}`;
                    ci.insertText = new vscode.SnippetString(`${findTag.attributes[i].name}="$1"`);
                    ci.kind = vscode.CompletionItemKind.Property;
                    if (findTag.attributes[i].isEvent) 
                        ci.kind = vscode.CompletionItemKind.Event;

                    items.push(ci);
                }
            }
        }
        return items;
    }
    private getValueCompletionItems(context: XamlContext): vscode.CompletionItem[] {
        const items: vscode.CompletionItem[] = [];
        const types = XamlController.getTypes(context.attributeContext?.parent?.namespace);
        const findTag = types.find(t => t.name === context.attributeContext?.parent?.name);
        if (findTag !== undefined) {
            const findProp = findTag.attributes.find((a: any) => a.name === context.attributeContext?.name);
            if (findProp !== undefined) {
                if (Array.isArray(findProp.type)) {
                    for (let i = 0; i < findProp.type.length; i++) {
                        items.push(new vscode.CompletionItem(findProp.type[i], vscode.CompletionItemKind.Enum));
                    }
                }
            }
        }
        return items;
    }
}
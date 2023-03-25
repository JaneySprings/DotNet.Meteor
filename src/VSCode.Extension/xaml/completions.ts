import { XamlService, languageId  } from './service';
import { ContextService } from './context';
import * as vscode from 'vscode';


export class XamlCompletionItemProvider implements vscode.CompletionItemProvider {
    async provideCompletionItems (textDocument: vscode.TextDocument, position: vscode.Position, token: vscode.CancellationToken, _context: vscode.CompletionContext): Promise<vscode.CompletionItem[] | vscode.CompletionList> {
        await XamlService.generate();

        const documentContent = textDocument.getText();
        const offset = textDocument.offsetAt(position);
        const scope = await ContextService.getContext(documentContent, offset);
        let completionItems: vscode.CompletionItem[] = [];

        if (textDocument.languageId === languageId && textDocument.fileName.includes(".xaml")) {
            if (scope.tagContext === undefined)
                return [];
            
            // Element
            if (scope.context === "element") {
                const xmlns = ContextService.getXmlns(documentContent, scope.tagContext.namespace);
                const types = XamlService.getTypes(xmlns);
                for (var i = 0; i < types.length; i++) {
                    completionItems.push(CompletionCreator.element(types[i]));
                }
            // Attribute
            } else if (scope.context === "attribute") {
                if (scope.attributeContext?.class !== undefined) {
                    // Attached property
                    const propXmlns = ContextService.getXmlns(documentContent, scope.attributeContext.namespace);
                    const propTypes = XamlService.getTypes(propXmlns);
                    const propType = propTypes.find(t => t.name === scope.attributeContext?.class);
                    for (let i = 0; i < propType.attributes.length; i++) {
                        if (propType.attributes[i].isAttached) {
                            completionItems.push(CompletionCreator.property(propType.attributes[i]));
                        }
                    }
                } else {
                    // All properties
                    const xmlns = ContextService.getXmlns(documentContent, scope.tagContext?.namespace);
                    const types = XamlService.getTypes(xmlns);
                    const findTag = types.find(t => t.name === scope.tagContext?.name);
                    if (findTag !== undefined) {
                        for (let i = 0; i < findTag.attributes.length; i++) {
                            completionItems.push(CompletionCreator.property(findTag.attributes[i], !scope.attributeContext?.specific));
                        }
                    }
                    const propXmlns = ContextService.getXmlns(documentContent, scope.attributeContext?.namespace);
                    const attachedClasses = XamlService.getAttachedTypes(propXmlns);
                    for (let i = 0; i < attachedClasses.length; i++) {
                        completionItems.push(CompletionCreator.attachedClass(attachedClasses[i]));
                    }
                }
            // Value
            } else if (scope.context === "value") {
                let findProp: any = undefined;
                if (scope.attributeContext?.class !== undefined) {
                    // Attached property
                    const propXmlns = ContextService.getXmlns(documentContent, scope.attributeContext?.namespace);
                    const propTypes = XamlService.getTypes(propXmlns);
                    const findTag = propTypes.find(t => t.name === scope.attributeContext?.class);
                    if (findTag !== undefined) {
                        findProp = findTag.attributes.find((t: { name: any; }) => t.name === scope.attributeContext?.name);
                    }
                } else {
                    // All properties
                    const propXmlns = ContextService.getXmlns(documentContent, scope.tagContext?.namespace);
                    const propTypes = XamlService.getTypes(propXmlns);
                    const findTag = propTypes.find(t => t.name === scope.tagContext?.name);
                    if (findTag !== undefined) {
                        findProp = findTag.attributes.find((t: { name: any; }) => t.name === scope.attributeContext?.name);
                    }
                }
                if (findProp !== undefined) {
                    if (Array.isArray(findProp.type)) {
                        for (let i = 0; i < findProp.type.length; i++) {
                            completionItems.push(CompletionCreator.value(findProp.type[i]));
                        }
                    }
                }
            }
        }

        return completionItems;
    }
}

class CompletionCreator {
    public static element(type: any): vscode.CompletionItem {
        const ci = new vscode.CompletionItem(type.name, vscode.CompletionItemKind.Class);
        ci.detail = `Class ${type.namespace}.${type.name}`;
        ci.documentation = type.doc;
        ci.insertText = new vscode.SnippetString(`${type.name} $1>`);
        return ci;
    }

    public static attachedClass(type: any): vscode.CompletionItem {
        const ci = new vscode.CompletionItem(type.name, vscode.CompletionItemKind.Module);
        ci.detail = `Class ${type.namespace}.${type.name}`;
        ci.documentation = type.doc;
        ci.insertText = new vscode.SnippetString(`${type.name}.$1`);
        return ci;
    }

    public static property(attr: any, format: boolean = true): vscode.CompletionItem {
        const ci = new vscode.CompletionItem(attr.name);
        if (format)
            ci.insertText = new vscode.SnippetString(`${attr.name}="$1"`);
        ci.documentation = attr.doc;
        if (attr.isEvent !== undefined && attr.isEvent) {
            ci.detail = `Event ${attr.namespace}.${attr.name}`;
            ci.kind = vscode.CompletionItemKind.Event;
        } else {
            ci.detail = `Property ${attr.namespace}.${attr.name}`;
            ci.kind = vscode.CompletionItemKind.Property;
        }
        return ci;
    }

    public static value(value: string): vscode.CompletionItem {
        return new vscode.CompletionItem(value, vscode.CompletionItemKind.Enum);
    }
}

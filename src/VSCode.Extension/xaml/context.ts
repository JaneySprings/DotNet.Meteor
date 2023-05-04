import { XamlContext, XamlScope, Context } from './types';


export class ContextService {
    public static getContext(content: string, offset: number): XamlContext | undefined {
        const context = new XamlContext();
        const documentPart = content.substring(0, offset);
        const contextTagStartIndex = documentPart.lastIndexOf('<');
        if (contextTagStartIndex < 0 || contextTagStartIndex > offset)
            return undefined;
        // If the tag is closed, we don't need to provide any context
        const documentSpan = documentPart.substring(contextTagStartIndex, offset);
        if (documentSpan.includes('>') || documentSpan.includes('</'))
            return undefined;
        // Extract all xmlns definitions from the document
        const xmlnsMatches = content.matchAll(/xmlns:([^=]*)="([^"]*)"/g);
        const xmlnsMatch = content.match(/xmlns="([^"]*)"/);
        if (xmlnsMatch !== null && xmlnsMatch.length == 2)
            context.imports[''] = xmlnsMatch[1];
        for (const match of xmlnsMatches) 
            context.imports[match[1]] = match[2];
        // Simple tag definition
        if (!documentSpan.includes(' ')) {
            const tagRawValue = documentSpan.substring(1);
            context.tagContext = ContextService.getTagContext(tagRawValue, content, context);
            context.scope = XamlScope.Tag;
            if (context.tagContext.name?.includes('.')) {
                const tokens = context.tagContext.name.split('.');
                context.tagContext.name = tokens[0];
                context.attributeContext = ContextService.getAttributeContext(tokens[1], content, context);
                context.attributeContext.parent = context.tagContext;
                context.scope = XamlScope.Multiline;
            }
        }
        // Tag with attributes
        if (documentSpan.includes(' ')) {
            const tagRawValue = documentSpan.substring(1, documentSpan.indexOf(' ')).trim();
            const attributeSpan = documentSpan.substring(documentSpan.lastIndexOf(' ') + 1);
            const equalIndex = attributeSpan.lastIndexOf('=');
            const attributeEndIndex = equalIndex === -1 ? undefined : equalIndex;
            const attributeRawValue = attributeSpan.substring(0, attributeEndIndex).trim();
            context.tagContext = ContextService.getTagContext(tagRawValue, content, context);
            context.attributeContext = ContextService.getAttributeContext(attributeRawValue, content, context);
            context.scope = XamlScope.Attribute;
            if (context.attributeContext.parent === undefined)
                context.attributeContext.parent = context.tagContext;

            if (context.attributeContext.parent !== context.tagContext)
                context.scope = XamlScope.Static;
        }
        
        if ((documentSpan.match(/"/g)?.length ?? 0) % 2 !== 0)
            context.scope = XamlScope.Value;

        return context;
    }

    private static getTagContext(rawValue: string, content: string, xamlContext: XamlContext): Context {
        const context = new Context();
        context.name = rawValue;
        if (rawValue.includes(':')) {
            const tokens = rawValue.split(":");
            context.prefix = tokens[0];
            context.name = tokens[1];
        } 

        context.namespace = xamlContext.imports[context.prefix ?? ''];
        return context;
    }

    private static getAttributeContext(rawValue: string, content: string, xamlContext: XamlContext): Context {
        const context = new Context();
        context.name = rawValue;
        if (rawValue.includes(':')) {
            const tokens = rawValue.split(":");
            context.prefix = tokens[0];
            context.name = tokens[1];
        } 

        context.namespace = xamlContext.imports[context.prefix ?? ''];
        if (context.name.includes('.')) {
            const nameTokens = context.name.split('.');
            context.parent = ContextService.getTagContext(nameTokens[0], content, xamlContext);
            context.name = nameTokens[1];
        }
        
        return context;
    }
}
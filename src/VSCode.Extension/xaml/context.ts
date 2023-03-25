import * as sax from 'sax';
import { BaseContext, XamlContext } from './types';


export class ContextService {
    public static async getContext(content: string, offset: number): Promise<XamlContext> {
        const parser = sax.parser(true);
        return await new Promise<XamlContext>(
            (resolve) => {
                const result = new XamlContext();
                parser.onerror = () => parser.resume();
                parser.onopentagstart = () => {
                    if (parser.position < offset)
                        return;

                    if (parser.tag.name.includes('.')) {
                        const parts = parser.tag.name.split('.');
                        result.tagContext = new BaseContext(parts[0]);
                        result.attributeContext = new BaseContext(parts[1]);
                        result.attributeContext.specific = true;
                        result.context = 'attribute';
                    } else {
                        result.tagContext = new BaseContext(parser.tag.name);
                        result.context = 'element';
                    }

                    parser.end();
                };
                parser.onattribute = () => {
                    if (parser.position < offset)
                        return;
                    
                    let lastAttr = undefined;
                    for (let key in parser.tag.attributes)
                        lastAttr = key;
                    
                    result.tagContext = new BaseContext(parser.tag.name);
                    result.attributeContext = new BaseContext(lastAttr ?? '');
                    result.context = 'attribute';

                    const substring = content.substring(offset, parser.position);
                    const quoteCount = substring.match(/"/g)?.length ?? 0;
                    if (quoteCount % 2 !== 0)
                        result.context = 'value';

                    parser.end();
                };
                parser.onend = () => resolve(result);
                parser.write(content).close();
            }
        );
    }

    public static getXmlns(content: string, prefix: string | undefined): string | undefined {
        const xmlnsMatch = prefix !== undefined 
            ? content.match(`xmlns:${prefix}="([^"]*)"`) 
            : content.match('xmlns="([^"]*)"');
        if (xmlnsMatch !== null && xmlnsMatch.length == 2) 
            return xmlnsMatch[1];
                
        return undefined;
    }
}
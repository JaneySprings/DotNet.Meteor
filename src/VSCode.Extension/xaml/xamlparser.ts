/* eslint-disable no-useless-escape */
import { XamlTagCollection, XamlDiagnosticData, XamlScope, CompletionString } from './types';

export default class XamlParser {
    public static async getXamlDiagnosticData (XamlContent: string, xsdTags: XamlTagCollection, nsMap: Map<string, string>, strict = true): Promise<XamlDiagnosticData[]> {
        // eslint-disable-next-line @typescript-eslint/no-var-requires
        const sax = require("sax");
        const parser = sax.parser(true);

        return await new Promise<XamlDiagnosticData[]>(
            (resolve) => {
                const result: XamlDiagnosticData[] = [];
                const nodeCacheAttributes = new Map<string, CompletionString[]>();
                const nodeCacheTags = new Map<string, CompletionString | undefined>();

                const getAttributes = (nodeName: string): CompletionString[] | undefined => {
                    if (!nodeCacheAttributes.has(nodeName)) {
                        nodeCacheAttributes.set(nodeName, xsdTags.loadAttributesEx(nodeName, nsMap));
                    }

                    return nodeCacheAttributes.get(nodeName);
                };

                const getTag = (nodeName: string): CompletionString | undefined => {
                    if (!nodeCacheTags.has(nodeName)) {
                        nodeCacheTags.set(nodeName, xsdTags.loadTagEx(nodeName, nsMap));
                    }

                    return nodeCacheTags.get(nodeName);
                };

                parser.onerror = () => {
                    if (undefined === result.find(e => e.line === parser.line)) {
                        result.push({
                            line: parser.line,
                            column: parser.column,
                            message: parser.error.message,
                            severity: strict ? "error" : "warning"
                        });
                    }
                    parser.resume();
                };

                parser.onopentag = (tagData: { name: string, isSelfClosing: boolean, attributes: Map<string, string>; }) => {
                    const nodeNameSplitted: string[] = tagData.name.split('.');

                    if (getTag(nodeNameSplitted[0]) !== undefined) {
                        const schemaTagAttributes = getAttributes(nodeNameSplitted[0]) ?? [];
                        nodeNameSplitted.shift();

                        const XamlAllowed: string[] = [":schemaLocation", ":noNamespaceSchemaLocation", "Xaml:space"];
                        Object.keys(tagData.attributes).concat(nodeNameSplitted).forEach((a: string) => {
                            if (schemaTagAttributes.findIndex(sta => sta.name === a) < 0 && !a.includes(":!") &&
                                a !== "Xamlns" && !a.startsWith("Xamlns:") &&
                                XamlAllowed.findIndex(all => a.endsWith(all)) < 0) {
                                result.push({
                                    line: parser.line,
                                    column: parser.column,
                                    message: `Unknown Xaml attribute '${a}' for tag '${tagData.name}'`,
                                    severity: strict ? "info" : "hint"
                                });
                            }
                        });
                    } else if (!tagData.name.includes(":!") && xsdTags.length > 0) {
                        result.push({
                            line: parser.line,
                            column: parser.column,
                            message: `Unknown Xaml tag '${tagData.name}'`,
                            severity: strict ? "info" : "hint"
                        });
                    }
                };

                parser.onend = () => {
                    resolve(result);
                };

                parser.write(XamlContent).close();
            });
    }

    public static async getNamespaceMapping (XamlContent: string): Promise<Map<string, string>> {
        // eslint-disable-next-line @typescript-eslint/no-var-requires
        const sax = require("sax");
        const parser = sax.parser(true);

        return await new Promise<Map<string, string>>(
            (resolve) => {
                const result: Map<string, string> = new Map<string, string>();

                parser.onerror = () => {
                    parser.resume();
                };

                parser.onattribute = (attr: { name: string, value: string; }) => {
                    if (attr.name.startsWith("Xamlns:")) {
                        result.set(attr.value, attr.name.substring("Xamlns:".length));
                    }
                };

                parser.onend = () => {
                    resolve(result);
                };

                parser.write(XamlContent).close();
            });
    }

    public static async getScopeForPosition (XamlContent: string, offset: number): Promise<XamlScope> {
        // eslint-disable-next-line @typescript-eslint/no-var-requires
        const sax = require("sax");
        const parser = sax.parser(true);

        return await new Promise<XamlScope>(
            (resolve) => {
                let result: XamlScope;
                let previousStartTagPosition = 0;
                const updatePosition = (): void => {
                    if ((parser.position >= offset) && result == null) {
                        let content = XamlContent.substring(previousStartTagPosition, offset);
                        content = content.lastIndexOf("<") >= 0 ? content.substring(content.lastIndexOf("<")) : content;

                        const normalizedContent = content.concat(" ").replace("/", "").replace("\t", " ").replace("\n", " ").replace("\r", " ");
                        const tagFullName = content.substring(1, normalizedContent.indexOf(" "));
                        const tagTokens = tagFullName.split(':');
                        const tagName = tagTokens.length == 2 ? tagTokens[1] : tagTokens[0];
                        const tagAttr = normalizedContent.match(/ .*?(?==)/g);

                        result = {
                            tagName: /^[a-zA-Z0-9_:\.\-]*$/.test(tagName) ? tagName : undefined,
                            tagPrefix: tagTokens.length == 2 ? tagTokens[0] : undefined,
                            tagAttribute: (tagAttr !== null && tagAttr.length > 0) ? tagAttr[tagAttr.length - 1].trim() : undefined,
                            context: undefined
                        };

                        if (content.lastIndexOf("=") === (parser.position - 1)) {
                            result.context = "value";
                        } else if (content.lastIndexOf(">") >= content.lastIndexOf("<")) {
                            result.context = "text";
                        } else {
                            const lastTagText = content.substring(content.lastIndexOf("<"));
                            if (!/\s/.test(lastTagText)) {
                                result.context = "element";
                            } else if ((lastTagText.split(`"`).length % 2) !== 0) {
                                result.context = "attribute";
                            } else {
                                result.context = "value";
                            }
                        }
                    }

                    previousStartTagPosition = parser.startTagPosition - 1;
                };

                parser.onerror = () => parser.resume();
                parser.ontext = () => updatePosition();
                parser.onopentagstart = () => updatePosition();
                parser.onattribute = () => updatePosition();
                parser.onclosetag = () => updatePosition();
                parser.onend = () => {
                    if (result === undefined) 
                        resolve(new XamlScope());
                    else resolve(result);
                };

                parser.write(XamlContent).close();
            }
        );
    }
    public static getXmlnsForPrefix(xamlContent: string, xmlPrefix: string | undefined): string | undefined {
        const xmlnsMatch = xmlPrefix !== undefined 
            ? xamlContent.match(`xmlns:${xmlPrefix}="([^"]*)"`) 
            : xamlContent.match('xmlns="([^"]*)"');
        if (xmlnsMatch !== null && xmlnsMatch.length == 2) 
            return xmlnsMatch[1];
                
        return undefined;
    }
}

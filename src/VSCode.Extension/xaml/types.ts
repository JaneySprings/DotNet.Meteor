import * as vscode from 'vscode';

export class CompletionString {
    constructor (
        public name: string,
        public namespace?: string,
        public comment?: string,
    ) {}
}

export class XamlTag {
    tag!: CompletionString;
    base!: string[];
    attributes!: CompletionString[];
    visible!: boolean;
}

export class XamlSchemaProperties {
    schemaUri!: vscode.Uri;
    parentSchemaUri!: vscode.Uri;
    xsdContent!: string;
    tagCollection!: XamlTagCollection;
}

export class XamlDiagnosticData {
    line!: number;
    column!: number;
    message!: string;
    severity!: "error" | "warning" | "info" | "hint";
}

export class XamlScope {
    tagName: string | undefined;
    tagAttribute: string | undefined;
    tagPrefix: string | undefined;
    context: "element" | "attribute" | "text" | "value" | undefined;
}


export class XamlTagCollection extends Array<XamlTag> {
    private readonly nsMap: Map<string, string> = new Map<string, string>();

    setNsMap (xsdNsTag: string, xsdNsStr: string): void {
        this.nsMap.set(xsdNsTag, xsdNsStr);
    }

    loadAttributesEx (tagName: string | undefined, localXamlMapping: Map<string, string>): CompletionString[] {
        if (tagName !== undefined) {
            const fixedNames = this.fixNsReverse(tagName, localXamlMapping);
            return fixedNames.flatMap(fixn => this.loadAttributes(fixn));
        }

        return [];
    }

    loadTagEx (tagName: string | undefined, localXamlMapping: Map<string, string>): CompletionString | undefined {
        if (tagName !== undefined) {
            const fixedNames = this.fixNsReverse(tagName, localXamlMapping);
            return this.find(e => fixedNames.includes(e.tag.name))?.tag;
        }

        return undefined;
    }

    loadAttributes (tagName: string | undefined, handledNames: string[] = []): CompletionString[] {
        const tagNameCompare = (a: string, b: string): boolean => a === b || b.endsWith(`:${a}`);

        const result: CompletionString[] = [];
        if (tagName !== undefined) {
            handledNames.push(tagName);
            const currentTags = this.filter(e => tagNameCompare(e.tag.name, tagName));
            if (currentTags.length > 0) {
                result.push(...currentTags.flatMap(e => e.attributes));
                result.push(...currentTags.flatMap(e =>
                    e.base.filter(b => !handledNames.includes(b))
                        .flatMap(b => this.loadAttributes(b))));
            }
        }
        return result;
    }

    fixNsReverse (XamlString: string, localXamlMapping: Map<string, string>): string[] {
        const arr = XamlString.split(":");
        const XamlStrings = new Array<string>();

        localXamlMapping.forEach((v, k) => {
            if (v === arr[0]) {
                this.nsMap.forEach((v2, k2) => {
                    if (v2 === k) {
                        XamlStrings.push(`${k2}:${arr[1]}`);
                    }
                });
            }
        });
        XamlStrings.push(arr[arr.length - 1]);

        return XamlStrings;
    }
}

export class XamlSchemaPropertiesArray extends Array<XamlSchemaProperties> {
    filterUris (uris: vscode.Uri[]): XamlSchemaProperties[] {
        return this.filter(e => uris
            .find(u => u.toString() === e.parentSchemaUri.toString()) !== undefined);
    }
}
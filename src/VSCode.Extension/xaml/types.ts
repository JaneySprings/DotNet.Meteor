export class XamlContext {
    public tagContext: Context | undefined;
    public attributeContext: Context | undefined;
    public imports: { [prefix: string]: string } = {};
    public scope: XamlScope | undefined;
}

export class Context {
    public name: string | undefined;
    public prefix: string | undefined;
    public parent: Context | undefined;
    public namespace: string | undefined;
}

export enum XamlScope {
    Tag = 0,
    Attribute = 1,
    Multiline = 2,
    Static = 3,
    Value = 4
}
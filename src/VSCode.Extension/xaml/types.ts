export class XamlContext {
    public tagContext: BaseContext | undefined;
    public attributeContext: BaseContext | undefined;
    public context: string | undefined;
}

export class BaseContext {
    public name: string;
    public class: string | undefined;
    public namespace: string | undefined;
    public specific: boolean | undefined;

    public constructor (name: string) {
        this.name = name;

        const nameTokens = name.split(":");
        if (nameTokens.length === 2) {
            this.namespace = nameTokens[0];
            this.name = nameTokens[1];
        }

        const classTokens = this.name.split(".");
        if (classTokens.length === 2) {
            this.class = classTokens[0];
            this.name = classTokens[1];
        }
    }
}
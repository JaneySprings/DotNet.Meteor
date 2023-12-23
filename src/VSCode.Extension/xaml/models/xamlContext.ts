import { Context } from "./context";
import { XamlScope } from "./xamlScope";

export class XamlContext {
    public tagContext: Context | undefined;
    public attributeContext: Context | undefined;
    public imports: { [prefix: string]: string } = {};
    public scope: XamlScope | undefined;
}

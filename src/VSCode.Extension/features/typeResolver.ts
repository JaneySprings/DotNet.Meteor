import * as res from '../resources/constants';
import * as vscode from 'vscode';

export class TypeResolver {
    public static feature : TypeResolver = new TypeResolver();
    public transportId: string | undefined;

    public async activate(context: vscode.ExtensionContext): Promise<void> {
        const dotrushExtension = vscode.extensions.getExtension(res.dotrushExtensionId);
        if (!dotrushExtension)
            return;

        this.transportId = `dotrush-${process.pid}`;
    }
}
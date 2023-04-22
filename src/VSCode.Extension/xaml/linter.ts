import { languageId } from './service';
import * as vscode from 'vscode';
import * as sax from 'sax';


export class XamlLinterProvider implements vscode.Disposable {
    private readonly diagnosticCollection: vscode.DiagnosticCollection;
    private currentDocument: vscode.TextDocument | undefined;
    private linterActive = false;

    constructor(context: vscode.ExtensionContext) {
        this.diagnosticCollection = vscode.languages.createDiagnosticCollection();
        context.subscriptions.push(vscode.workspace.onDidChangeTextDocument(async evnt => await this.validateDocument(evnt.document)));
        context.subscriptions.push(vscode.workspace.onDidOpenTextDocument(async doc => await this.validateDocument(doc, 100)));
    }

    public dispose (): void {
        this.diagnosticCollection.clear();
    }


    private async validateDocument(textDocument: vscode.TextDocument, timeout: number = 1500): Promise<void> {
        if (this.linterActive)
            return;

        this.currentDocument = textDocument;
        this.linterActive = true;
        await new Promise(resolve => setTimeout(resolve, timeout));
 
        try {
            await this.validate(this.currentDocument);
        } finally {
            this.linterActive = false;
        }
    }

    private async validate(document: vscode.TextDocument): Promise<void> {
        if (document.languageId !== languageId || !document.fileName.includes(".xaml")) 
            return;

        try {
            const diagnostics = await this.getDiagnostics(document);
            this.diagnosticCollection.set(document.uri, diagnostics);
        } finally {}
    }


    private async getDiagnostics(document: vscode.TextDocument): Promise<vscode.Diagnostic[]> {
        const parser = sax.parser(true);
        return await new Promise<vscode.Diagnostic[]>(
            (resolve) => {
                const result: vscode.Diagnostic[] = [];
                parser.onerror = () => {
                    const position = document.positionAt(parser.startTagPosition);
                    const diag = new vscode.Diagnostic(
                        new vscode.Range(
                            position, 
                            new vscode.Position(parser.line, parser.column)
                        ),
                        parser.error.message.replace(/\n/g, ' '),
                        vscode.DiagnosticSeverity.Error
                    );
                    result.push(diag);
                    parser.resume();
                }
                parser.onend = () => resolve(result);
                parser.write(document.getText()).close();
            }
        );
    }
}

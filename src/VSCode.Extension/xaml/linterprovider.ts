import * as vscode from 'vscode';
import { languageId } from './xamlservice';
import { XamlTagCollection, XamlSchemaPropertiesArray, XamlDiagnosticData } from './types';
import XamlSimpleParser from './xamlparser';

export default class XamlLinterProvider implements vscode.Disposable {
    private readonly documentListener: vscode.Disposable;
    private readonly diagnosticCollection: vscode.DiagnosticCollection;
    private delayCount: number = Number.MIN_SAFE_INTEGER;
    private textDocument!: vscode.TextDocument;
    private linterActive = false;

    constructor (protected extensionContext: vscode.ExtensionContext, protected schemaPropertiesArray: XamlSchemaPropertiesArray) {
        this.schemaPropertiesArray = schemaPropertiesArray;
        this.diagnosticCollection = vscode.languages.createDiagnosticCollection();

        this.documentListener = vscode.workspace.onDidChangeTextDocument(async evnt =>
            await this.triggerDelayedLint(evnt.document), this, this.extensionContext.subscriptions);

        vscode.workspace.onDidOpenTextDocument(async doc =>
            await this.triggerDelayedLint(doc, 100), this, extensionContext.subscriptions);

        vscode.workspace.onDidCloseTextDocument(doc =>
            this.cleanupDocument(doc), null, extensionContext.subscriptions);

        // eslint-disable-next-line @typescript-eslint/no-misused-promises
        vscode.workspace.textDocuments.forEach(async doc => await this.triggerDelayedLint(doc, 100), this);
    }

    public dispose (): void {
        this.documentListener.dispose();
        this.diagnosticCollection.clear();
    }

    private cleanupDocument (textDocument: vscode.TextDocument): void {
        this.diagnosticCollection.delete(textDocument.uri);
    }

    private async triggerDelayedLint (textDocument: vscode.TextDocument, timeout = 2000): Promise<void> {
        if (this.delayCount > Number.MIN_SAFE_INTEGER) {
            this.delayCount = timeout;
            this.textDocument = textDocument;
            return;
        }
        this.delayCount = timeout;
        this.textDocument = textDocument;

        const tick = 100;

        while (this.delayCount > 0 || this.linterActive) {
            await new Promise(resolve => setTimeout(resolve, tick));
            this.delayCount -= tick;
        }

        try {
            this.linterActive = true;
            await this.triggerLint(this.textDocument);
        } finally {
            this.delayCount = Number.MIN_SAFE_INTEGER;
            this.linterActive = false;
        }
    }

    private async triggerLint(textDocument: vscode.TextDocument): Promise<void> {
        if (textDocument.languageId !== languageId) 
            return;

        const diagnostics: vscode.Diagnostic[][] = new Array<vscode.Diagnostic[]>();
        try {
            const documentContent = textDocument.getText();
            const nsMap = await XamlSimpleParser.getNamespaceMapping(documentContent);
            const text = textDocument.getText();
            const plainXamlCheckResults = await XamlSimpleParser.getXamlDiagnosticData(text, new XamlTagCollection(), nsMap, false);
            diagnostics.push(this.getDiagnosticArray(plainXamlCheckResults));
            
            this.diagnosticCollection.set(textDocument.uri, diagnostics
                .reduce((prev, next) => prev.filter(dp => next.some(dn => dn.range.start.compareTo(dp.range.start) === 0))));
        } catch (err) {
            console.debug(err);
        }
    }

    private getDiagnosticArray (data: XamlDiagnosticData[]): vscode.Diagnostic[] {
        return data.map(r => {
            const position = new vscode.Position(r.line, r.column);
            const severity = (r.severity === "error") ? vscode.DiagnosticSeverity.Error
                : (r.severity === "warning") ? vscode.DiagnosticSeverity.Warning
                    : (r.severity === "info") ? vscode.DiagnosticSeverity.Information
                        : vscode.DiagnosticSeverity.Hint;

            return new vscode.Diagnostic(new vscode.Range(position, position), r.message, severity);
        });
    }
}

import { getRuntimeIdentifier, getExecutableExtension } from '../resources/runtime';
import * as vscode from 'vscode';
import * as path from 'path';

export class DebugAdapterDescriptorFactory implements vscode.DebugAdapterDescriptorFactory {
    private readonly extensionPath: string;

    constructor(extensionPath: string) {
        this.extensionPath = extensionPath;
    }

    createDebugAdapterDescriptor(session: vscode.DebugSession, executable: vscode.DebugAdapterExecutable | undefined): vscode.ProviderResult<vscode.DebugAdapterDescriptor> {
        const program = path.join(this.extensionPath, "extension", "bin", getRuntimeIdentifier(), "Debugger", "DotNet.Meteor.Debugger" + getExecutableExtension());
        return new vscode.DebugAdapterExecutable(program);
    }
}

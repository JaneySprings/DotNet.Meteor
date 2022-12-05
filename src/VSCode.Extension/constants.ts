import * as vscode from 'vscode';

export const extensionIdentifier: string = "nromanov.dotnet-meteor";
export const taskProviderType: string = "dotnet-meteor.build";
export const debuggerType: string = "dotnet-meteor.debug";
export const extensionPath: string = vscode.extensions.getExtension(extensionIdentifier)?.extensionPath ?? "?";

export class Command {
    public static readonly selectProject: string = "dotnet-meteor.selectActiveProject";
    public static readonly selectTarget: string = "dotnet-meteor.selectActiveConfiguration";
    public static readonly selectDevice: string = "dotnet-meteor.selectActiveDevice";
}
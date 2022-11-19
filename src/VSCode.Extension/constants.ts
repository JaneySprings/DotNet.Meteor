import * as vscode from 'vscode';

export const extensionIdentifier: string = "nromanov.dotnet-meteor";
export const extensionInstance: vscode.Extension<any> | undefined = vscode.extensions.getExtension(extensionIdentifier);

export class Command {
    public static readonly selectProject: string = "dotnet-meteor.select-project";
    public static readonly selectTarget: string = "dotnet-meteor.select-target";
    public static readonly selectDevice: string = "dotnet-meteor.select-device";
}
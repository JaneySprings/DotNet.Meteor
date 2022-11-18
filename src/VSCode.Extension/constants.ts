import * as vscode from 'vscode';

export const extensionIdentifier: string = "nromanov.dotnet-meteor";
export const extensionInstance: vscode.Extension<any> | undefined = vscode.extensions.getExtension(extensionIdentifier);

export const commandSelectProjectIdentifier: string = "dotnet-meteor.select-project";
export const commandSelectTargetIdentifier: string = "dotnet-meteor.select-target";
export const commandSelectDeviceIdentifier: string = "dotnet-meteor.select-device";
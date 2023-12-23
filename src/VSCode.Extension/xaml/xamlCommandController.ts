import { ProcessArgumentBuilder } from '../processes/processArgumentBuilder';
import { ProcessRunner } from '../processes/processRunner';
import * as res from '../resources/constants';
import * as vscode from 'vscode';
import * as path from "path";
import * as fs from "fs";

export class XamlCommandController {
    private static xamlToolPath: string;

    public static activate(context: vscode.ExtensionContext) {
        const qualifiedVersion = XamlCommandController.runtimeVersion() ?? "";
        const qualifiedVersionRegex = new RegExp('^\\d+\\.\\d+', ''); 
        const versionRegexCollection = qualifiedVersionRegex.exec(qualifiedVersion);
        const version = (versionRegexCollection && versionRegexCollection.length !== 0) 
            ? versionRegexCollection[0] : "6.0";

        const extensionPath = vscode.extensions.getExtension(`${res.extensionPublisher}.${res.extensionId}`)?.extensionPath ?? '';
        let extensionBinaryPath = path.join(extensionPath, "extension", "bin", "Xaml", `net${version}`);
        if (!fs.existsSync(extensionBinaryPath)) 
            extensionBinaryPath = path.join(extensionPath, "extension", "bin", "Xaml", "net6.0");
        
        const executableExtension = process.platform === 'win32' ? '.exe' : '';
        XamlCommandController.xamlToolPath = path.join(extensionBinaryPath, "DotNet.Meteor.Xaml" + executableExtension);
    }

    public static async xamlSchema(path: string): Promise<boolean>  {
        return await ProcessRunner.runAsync<boolean>(new ProcessArgumentBuilder(XamlCommandController.xamlToolPath)
            .appendQuoted(path));
    }
    private static runtimeVersion(): string | undefined {
        return ProcessRunner.runSync("dotnet", "--version");
    }
}
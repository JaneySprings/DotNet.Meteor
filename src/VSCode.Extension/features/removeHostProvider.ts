import { ConfigurationController } from '../controllers/configurationController';
import { StateController } from '../controllers/stateController';
import { ProcessArgumentBuilder } from '../interop/processArgumentBuilder';
import * as res from '../resources/constants';
import * as vscode from 'vscode';

export class RemoteHostProvider {
    public static feature: RemoteHostProvider = new RemoteHostProvider();
    private static credentialsKey: string = `${res.extensionId}.remoteHostCredentials`;

    public async activate(context: vscode.ExtensionContext): Promise<void> {
        context.subscriptions.push(vscode.commands.registerCommand(res.commandIdPairToMac, async () => await RemoteHostProvider.feature.pairToMac()));
    }

    public async pairToMac(): Promise<void> {
        if (!ConfigurationController.onWindows) {
            await vscode.window.showErrorMessage(res.messageRemoteHostNotSupported);
            return;
        }

        const address = await vscode.window.showInputBox({ placeHolder: res.messageRemoteHostAddress });
        if (address === undefined || address === '')
            return;

        const username = await vscode.window.showInputBox({ placeHolder: res.messageRemoteHostUsername });
        if (username === undefined || username === '')
            return;

        const password = await vscode.window.showInputBox({ placeHolder: res.messageRemoteHostPassword, password: true });
        if (password === undefined || password === '')
            return;

        const credentials: RemoteHostCredentials = { address, username, password };
        StateController.putGlobal(RemoteHostProvider.credentialsKey, credentials);
    }
    public connect(builder: ProcessArgumentBuilder): ProcessArgumentBuilder {
        const credentials = StateController.getGlobal<RemoteHostCredentials>(RemoteHostProvider.credentialsKey);
        if (credentials === undefined)
            return builder;

        builder.append(`-p:ServerAddress=${credentials.address}`);
        builder.append(`-p:ServerUser=${credentials.username}`);
        builder.append(`-p:ServerPassword=${credentials.password}`);
        builder.append('-p:ContinueOnDisconnected=false');
        return builder;
    }
}

interface RemoteHostCredentials {
    address: string;
    username: string;
    password: string;
}
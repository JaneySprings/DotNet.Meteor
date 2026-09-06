import { ConfigurationController } from '../controllers/configurationController';
import * as res from '../resources/constants';
import * as vscode from 'vscode';

export class CoreClrConfigurationProvider implements vscode.DebugConfigurationProvider {
	async resolveDebugConfiguration(folder: vscode.WorkspaceFolder | undefined,
		config: vscode.DebugConfiguration,
		token?: vscode.CancellationToken): Promise<vscode.DebugConfiguration | undefined> {

		config.remoteCoreclrTarget = ConfigurationController.getSettingOrDefault<string>(res.configIdRemoteCoreclrTarget);
		config.remoteCoreclrHost = ConfigurationController.getSettingOrDefault<string>(res.configIdRemoteCoreclrHost);

		if (!ConfigurationController.project?.path) {
			vscode.window.showErrorMessage(res.messageNoProjectFound, { modal: true });
			return undefined;
		}
		if (!ConfigurationController.device?.platform) {
			vscode.window.showErrorMessage(res.messageNoDeviceFound, { modal: true });
			return undefined;
		}
		if (!ConfigurationController.targetFramework) {
			vscode.window.showErrorMessage(res.messageNoFrameworkFound, { modal: true });
			return undefined;
		}
		if (!config.remoteCoreclrTarget) {
			vscode.window.showErrorMessage(res.messageMissingCoreclrTarget, { modal: true });
			return undefined;
		}
		if (!config.remoteCoreclrHost) {
			vscode.window.showErrorMessage(res.messageMissingCoreclrHost, { modal: true });
			return undefined;
		}

		// Special case for net10.0-ios + mac
		if (ConfigurationController.targetFramework === 'net10.0-ios') {
			vscode.window.showErrorMessage(res.messageClrIosNotSupported, { modal: true });
			return undefined;
		}
		if (ConfigurationController.targetFramework === 'net10.0-maccatalyst') {
			vscode.window.showErrorMessage(res.messageClrMacNotSupported, { modal: true });
			return undefined;
		}

		if (!config.type && !config.request && !config.name) {
			config.type = res.debuggerMeteorId;
			config.name = '.NET Meteor Debugger';
			config.request = 'launch';
			config.preLaunchTask = `${res.extensionId}: Build`
		}

		const project = ConfigurationController.project!;
		const configuration = ConfigurationController.configuration!;
		const device = ConfigurationController.device!;

		if (config.program === undefined)
			config.program = ConfigurationController.getProgramPath(project, configuration, device);

		// config.sourceFileMap = CoreClrConfigurationProvider.getSetting('');
		config.justMyCode = CoreClrConfigurationProvider.getSetting('debugger.projectAssembliesOnly');
		config.enableStepFiltering = CoreClrConfigurationProvider.getSetting('debugger.stepOverPropertiesAndOperators');
		config.symbolOptions = {
			searchPaths: CoreClrConfigurationProvider.getSetting('debugger.symbolSearchPaths'),
			searchMicrosoftSymbolServer: CoreClrConfigurationProvider.getSetting('debugger.searchMicrosoftSymbolServer'),
			searchNuGetOrgSymbolServer: CoreClrConfigurationProvider.getSetting('debugger.searchNugetSymbolServer'),
		}
		config.sourceLinkOptions = {
			"*": { enabled: CoreClrConfigurationProvider.getSetting('debugger.automaticSourcelinkDownload') }
		}

		if (!ConfigurationController.isWindows()) {
			config.coreClrMobileDebuggerOptions = {
				runtimeIdentifier: device.runtime_id,
				platform: device.platform,
				ip: "127.0.0.1",
				port: ConfigurationController.getDebuggingPort(),
				assetsPath: ConfigurationController.getAssetsPath(config.program, project, configuration),
				uninstallApp: ConfigurationController.getSetting(res.configIdUninstallApplication, true),
				device: ConfigurationController.isAndroid() && device.is_emulator ? device.name : device.serial,
				isDevice: !device.is_emulator,
				tcpTunnel: [
					ConfigurationController.getSetting(res.configIdHotReloadHostPort, 9988)
				]
			}
		}

		return config;
	}

	private static getSetting<TResult>(id: string): TResult | undefined {
		return vscode.workspace.getConfiguration(res.configDotRushId).get(id);
	}
}
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

		if (!config.type && !config.request && !config.name) {
			config.preLaunchTask = `${res.extensionId}: ${res.taskDefinitionDefaultTargetCapitalized}`
			config.name = res.debuggerMeteorTitle;
			config.type = res.debuggerMeteorId;
			config.request = 'launch';
		}

		const project = ConfigurationController.project!;
		const configuration = ConfigurationController.configuration!;
		const device = ConfigurationController.device!;

		if (config.program === undefined)
			config.program = ConfigurationController.getProgramPath(project, configuration, device);

		config.justMyCode = ConfigurationController.getSettingOrDefault(res.configIdProjectAssembliesOnly);
		config.enableStepFiltering = ConfigurationController.getSettingOrDefault(res.configIdStepOverPropertiesAndOperators);
		config.sourceFileMap = ConfigurationController.getSettingOrDefault(res.configIdSourceCodeMappings);
		config.symbolOptions = {
			searchPaths: ConfigurationController.getSettingOrDefault(res.configIdSymbolSearchPaths),
			searchMicrosoftSymbolServer: ConfigurationController.getSettingOrDefault(res.configIdSearchMicrosoftSymbolServer),
		}
		config.sourceLinkOptions = {
			"*": { enabled: ConfigurationController.getSettingOrDefault(res.configIdAutomaticSourcelinkDownload) }
		}

		if (!ConfigurationController.isWindows()) {
			config.coreClrMobileDebuggerOptions = {
				runtimeIdentifier: device.runtime_id,
				platform: device.platform,
				ip: "127.0.0.1",
				port: ConfigurationController.getDebuggingPort(),
				assetsPath: ConfigurationController.getAssetsPath(config.program, project, configuration, device),
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
}
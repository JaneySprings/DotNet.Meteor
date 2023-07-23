import { ConfigurationController } from './configuration';
import { UIController } from './controller';
import { ExtensionContext } from 'vscode';
import * as models from "./models"


export class StateController {
    private static context: ExtensionContext | undefined;


    public static activate(context: ExtensionContext) {
        StateController.context = context;
    }
    public static deactivate() {
        StateController.context = undefined;
    }


    public static load() {
        if (StateController.context === undefined)
            return;

        const project = StateController.context.workspaceState.get<string>('project');
        const device = StateController.context.workspaceState.get<string>('device');
        const target = StateController.context.workspaceState.get<models.Target>('target');

        ConfigurationController.device = UIController.devices.find(it => it.name === device);
        ConfigurationController.project = UIController.projects.find(it => it.path === project);
        ConfigurationController.target = target;
    }
    public static saveProject() {
        if (StateController.context !== undefined)
             StateController.context.workspaceState.update('project', ConfigurationController.project?.path);
    }
    public static saveDevice() {
        if (StateController.context !== undefined)
            StateController.context.workspaceState.update('device', ConfigurationController.device?.name);
    }
    public static saveTarget() {
        if (StateController.context !== undefined)
            StateController.context.workspaceState.update('target', ConfigurationController.target);
    }
}
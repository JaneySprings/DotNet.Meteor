import { Configuration } from './configuration';
import { UIController } from './controller';
import * as models from "./models"
import * as vscode from 'vscode';


export class StateManager {
    private static context: vscode.ExtensionContext | undefined;


    public static activate(context: vscode.ExtensionContext) {
        StateManager.context = context;
    }
    public static deactivate() {
        StateManager.context = undefined;
    }


    public static load() {
        if (StateManager.context === undefined)
            return;

        const project = StateManager.context.workspaceState.get<string>('project');
        const device = StateManager.context.workspaceState.get<string>('device');
        const target = StateManager.context.workspaceState.get<models.Target>('target');

        Configuration.device = UIController.devices.find(it => it.name === device);
        Configuration.project = UIController.projects.find(it => it.path === project);
        Configuration.target = target;
    }
    public static save() {
        if (StateManager.context === undefined)
            return;

        StateManager.context.workspaceState.update('project', Configuration.project?.path);
        StateManager.context.workspaceState.update('device', Configuration.device?.name);
        StateManager.context.workspaceState.update('target', Configuration.target);
    }
}
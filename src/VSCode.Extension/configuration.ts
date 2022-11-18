import { Project, Device } from "./models"
import { Interface } from './interface';


export enum Target {
    Debug = "Debug",
    Release = "Release"
}

export class Configuration {
    public static workspaceProjects: Project[] = [];
    public static mobileDevices: Device[] = [];

    public static selectedProject: Project | undefined;
    public static selectedDevice: Device | undefined;
    public static selectedTarget: Target | undefined;

    public static selectProject(item: Project) {
        Configuration.selectedProject = item;
        Interface.updateProjectsStatusItem();
    }
    public static selectTarget(target: Target) {
        Configuration.selectedTarget = target;
        Interface.updateTargetStatusItem();
    }
    public static selectDevice(item: Device) {
        Configuration.selectedDevice = item;
        Interface.updateDeviceStatusItem();
    }
} 
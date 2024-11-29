import { QuickPickItem } from "vscode";
import { Project } from "./project";

export class ProjectItem implements QuickPickItem {
    label: string;
    description: string;
    // detail: string;
    item: Project;

    constructor(project: Project) {
        this.label = project.name;
        // this.detail = project.path;
        this.description = project.frameworks?.join('  ');
        this.item = project;
    }
}

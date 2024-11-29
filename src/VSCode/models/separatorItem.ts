import { QuickPickItem, QuickPickItemKind } from 'vscode';

export class SeparatorItem implements QuickPickItem {
    kind: QuickPickItemKind = QuickPickItemKind.Separator;
    label: string;

    constructor(label: string | undefined) {
        this.label = label ?? '';
    }
}

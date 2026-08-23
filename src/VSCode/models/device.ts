import { QuickPickItem, QuickPickItemKind } from "vscode";
import { Icons } from "../resources/icons";

export interface Device {
    name: string | undefined;
    category: string | undefined;
    serial: string | undefined;
    platform: string | undefined;
    os_version: string | undefined;
    runtime_id: string | undefined;
    is_emulator: boolean | undefined;
    is_running: boolean | undefined;
    is_mobile: boolean | undefined;
}

export class DeviceItem implements QuickPickItem {
    label: string;
    description: string;
    item: Device;

    constructor(device: Device) {
        this.label = `${Icons.deviceState(device)} ${device.name}`;
        this.description = device.os_version ?? '';
        this.item = device;
    }
}
export class SeparatorItem implements QuickPickItem {
    kind: QuickPickItemKind = QuickPickItemKind.Separator;
    label: string;

    constructor(label: string | undefined) {
        this.label = label ?? '';
    }
}

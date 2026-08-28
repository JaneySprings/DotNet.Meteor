import { QuickPickItem, QuickPickItemKind } from "vscode";
import { Icons } from "../resources/icons";

export interface Device {
    name: string;
    category: string;
    platform: string;
    serial: string | undefined;
    os_version: string | undefined;
    runtime_id: string | undefined;
    is_emulator: boolean;
    is_running: boolean;
    is_mobile: boolean;
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

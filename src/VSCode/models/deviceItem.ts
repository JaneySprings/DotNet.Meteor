import { QuickPickItem } from "vscode";
import { Icons } from "../resources/icons";
import { Device } from "./device";

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

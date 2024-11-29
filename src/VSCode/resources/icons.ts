import { Device } from "../models/device";
import { ThemeIcon } from "vscode";

export class Icons {
    public static readonly project = "$(window)";
    public static readonly target = "$(window)";
    public static readonly device = "$(device-mobile)";
    public static readonly computer = "$(vm)";
    public static readonly active = "$(debug-disconnect)";
    public static readonly inactive = "$(device-mobile)";

    public static readonly module = new ThemeIcon("symbol-namespace");

    public static deviceState(device: Device): string {
        return device.is_running ? Icons.active : Icons.inactive;
    }
    public static deviceKind(device: Device): string {
        return device.is_mobile ? Icons.device : Icons.computer;
    }
}

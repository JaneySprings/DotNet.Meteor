import { Device } from "../models/device";

export class Icons {
    public static readonly project = "$(window)";
    public static readonly target = "$(window)";
    public static readonly device = "$(device-mobile)";
    public static readonly computer = "$(vm)";
    public static readonly active = "$(debug-disconnect)";
    public static readonly inactive = "$(device-mobile)";

    public static deviceState(device: Device): string {
        return device.is_running ? Icons.active : Icons.inactive;
    }
    public static deviceKind(device: Device): string {
        return device.is_mobile ? Icons.device : Icons.computer;
    }
}

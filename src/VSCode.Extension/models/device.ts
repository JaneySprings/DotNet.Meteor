
export interface Device {
    name: string | undefined;
    detail: string | undefined;
    serial: string | undefined;
    platform: string | undefined;
    os_version: string | undefined;
    runtime_id: string | undefined;
    is_emulator: boolean | undefined;
    is_running: boolean | undefined;
    is_mobile: boolean | undefined;
}

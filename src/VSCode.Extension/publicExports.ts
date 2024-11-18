
export class PublicExports {
    public static instance: PublicExports;

    public onActiveProjectChanged: EventHandler;
    public onActiveConfigurationChanged: EventHandler;
    public onActiveFrameworkChanged: EventHandler;
    public onActiveDeviceChanged: EventHandler;

    constructor() {
        PublicExports.instance = this;
        this.onActiveProjectChanged = new EventHandler();
        this.onActiveConfigurationChanged = new EventHandler();
        this.onActiveFrameworkChanged = new EventHandler();
        this.onActiveDeviceChanged = new EventHandler();
    }

    public invokeAll() {
        this.onActiveProjectChanged.invoke(undefined);
        this.onActiveConfigurationChanged.invoke(undefined);
        this.onActiveFrameworkChanged.invoke(undefined);
        this.onActiveDeviceChanged.invoke(undefined);
    }
}

class EventHandler {
    private callbacks: Array<(data: any) => void>;
    private delayedData: any | undefined;

    constructor() {
        this.callbacks = [];
    }

    public add(callback: (data: any) => void) {
        this.callbacks.push(callback);
        if (this.delayedData !== undefined) {
            callback(this.delayedData);
            this.delayedData = undefined;
        }
    }
    public remove(callback: (data: any) => void) {
        const index = this.callbacks.indexOf(callback);
        if (index != -1 && index < this.callbacks.length)
            this.callbacks.splice(index, 1);
    }
    public invoke(data: any) {
        if (this.callbacks.length === 0) {
            this.delayedData = data;
            return;
        }
        this.callbacks.forEach(callback => callback(data));
    }
}
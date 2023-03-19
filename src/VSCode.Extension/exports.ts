
export class PublicExports {
    public static instance: PublicExports;
    public projectChangedEventHandler: EventHandler;
    public targetChangedEventHandler: EventHandler;
    public deviceChangedEventHandler: EventHandler;

    constructor() {
        PublicExports.instance = this;
        this.projectChangedEventHandler = new EventHandler();
        this.targetChangedEventHandler = new EventHandler();
        this.deviceChangedEventHandler = new EventHandler();
    }
}


class EventHandler {
    private callbacks: Array<(data: any) => void>;

    constructor() {
        this.callbacks = [];
    }

    public add(callback: (data: any) => void) {
        this.callbacks.push(callback);
    }
    public remove(callback: (data: any) => void) {
        const index = this.callbacks.indexOf(callback);
        if (index != -1 && index < this.callbacks.length)
            this.callbacks.splice(index, 1);
    }
    public invoke(data: any) {
        this.callbacks.forEach(callback => callback(data));
    }
}
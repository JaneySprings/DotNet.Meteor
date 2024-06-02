
export class ProcessArgumentBuilder {
    private command: string;
    private arguments: string[];

    public constructor(command: string) {
        this.command = command;
        this.arguments = [];
    }

    public getCommand(): string {
        return this.command;
    }
    public getArguments(): string[] {
        return this.arguments;
    }

    public append(...args: string[]): ProcessArgumentBuilder {
        args.forEach(a => this.arguments.push(a));
        return this;
    }
    public appendQuoted(...args: string[]): ProcessArgumentBuilder {
        args.forEach(a => this.arguments.push(`"${a}"`));
        return this;
    }
    public conditional(arg: string, condition: () => any): ProcessArgumentBuilder {
        if (condition())
            this.arguments.push(arg);
        return this;
    }
    public override(arg: string): ProcessArgumentBuilder {
        const argName = arg.split("=")[0];
        const index = this.arguments.findIndex(a => a.startsWith(argName));
        if (index > -1) 
            this.arguments.splice(index, 1);
        this.arguments.push(arg);
        return this;
    }
    public build(): string {
        return `${this.command} ${this.arguments.join(" ")}`;
    }
}

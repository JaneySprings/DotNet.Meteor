import { execSync, exec } from 'child_process';


export class ProcessRunner {
    public static run<TModel>(builder: ProcessArgumentBuilder): TModel {
        const result = execSync(builder.build()).toString();
        return JSON.parse(result);
    }

    public static runAsync<TModel>(builder: ProcessArgumentBuilder, callback: (model: TModel) => any) {
        exec(builder.build(), (error, stdout, stderr) => {
            if (error) {
                console.error(error);
                process.exit(1);
            } else {
                const item: TModel = JSON.parse(stdout.toString());
                callback(item);
            }
        })
    }
}

export class ProcessArgumentBuilder {
    private args: string[] = [];

    public constructor(command: string) {
        this.args.push(command);
    }

    public append(...params: string[]): ProcessArgumentBuilder {
        for (let i = 0; i < params.length; i++) {
            this.args.push(params[i]);
        }
        return this;
    }
    public build(): string {
        return this.args.join(" ");
    }
}
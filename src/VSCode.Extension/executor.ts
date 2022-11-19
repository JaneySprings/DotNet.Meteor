import { execSync } from 'child_process';
import * as vscode from 'vscode';

export class ProcessRunner {
    public static run<TModel>(builder: ProcessArgumentBuilder): TModel {
        const result = this.execute(builder.build());
        return JSON.parse(result);
    }

    private static execute(command: string): string {
        const buffer = execSync(command);
        return buffer.toString();
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
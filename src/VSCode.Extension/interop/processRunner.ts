import { spawnSync, spawn } from 'child_process';
import { ProcessArgumentBuilder } from './processArgumentBuilder';

export class ProcessRunner {
    public static runSync(builder: ProcessArgumentBuilder): string | undefined {
        const result = spawnSync(builder.getCommand(), builder.getArguments());
        if (result.error) {
            console.error(result.error);
            return undefined;
        }
        return result.stdout.toString().trimEnd();
    }
    public static runAsync<TModel>(builder: ProcessArgumentBuilder): Promise<TModel> {
        return new Promise<TModel>((resolve, reject) => {
            const process = spawn(builder.getCommand(), builder.getArguments());
            process.stdout.on('data', (data) => {
                resolve(JSON.parse(data.toString()));
            });
            process.stderr.on('data', (data) => {
                console.error(data.toString());
                reject(data.toString());
            });
        });
    }
}

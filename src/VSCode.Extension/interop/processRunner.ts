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
            const child = spawn(builder.getCommand(), builder.getArguments(), {
                stdio: ['ignore', 'pipe', 'pipe'],
                detached: true,
            });
    
            let output = '';
            child.stdout?.on('data', (data) => {
                output += data.toString().trimEnd();
            });
            child.stderr?.on('data', (data) => {
                console.error(data.toString());
                reject(data.toString());
            });
            child.on('close', () => {
                resolve(JSON.parse(output));
            });
            child.unref();
        });
    }
}

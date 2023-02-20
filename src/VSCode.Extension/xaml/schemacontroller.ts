import { CommandInterface } from "../bridge";
import { Configuration } from "../configuration";


export class SchemaController {
    private static xamlSchemaAliases: any[] = [];

    public static xamlAliasByNamespace(namespace: string | undefined): any[] { 
        if (namespace === undefined)
            return [];

        const schema = this.xamlSchemaAliases.find(x => namespace.includes(x.xmlns));
        if (schema === undefined) 
            return [];

        return schema.types;
    }
    public static async prepareXamlSchemaAliases() {
        if (this.xamlSchemaAliases.length > 0) 
            return;

        const projectPath = Configuration.project?.path;
        if (projectPath === undefined)
            return;
        
        const result = await CommandInterface.xamlSchema(projectPath);
        if (result === false)
            return;
        
        const fs = require('fs');
        const path = require('path');
        if (fs.existsSync(CommandInterface.generatedPath) === false)
            return;

        const files = await fs.promises.readdir(CommandInterface.generatedPath);
        for (const file of files) {
            const filePath = path.join(CommandInterface.generatedPath, file);
            const dataArray = JSON.parse(fs.readFileSync(filePath));
            this.xamlSchemaAliases.push(dataArray);
        }
    }
    public static invalidate() {
        this.xamlSchemaAliases = [];
    }
}
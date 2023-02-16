import { CommandInterface } from "../bridge";
import { Configuration } from "../configuration";
import { XamlSchemaAlias } from "./types";


export class SchemaController {
    private static microsoftMauiSchemaName: string = "Microsoft.Maui.Controls";
    private static xamlSchemaAliases: XamlSchemaAlias[] = [];

    public static xamlAliasByName(name: string | undefined): any[] { 
        const query = name ?? this.microsoftMauiSchemaName;
        const schema = this.xamlSchemaAliases.find(x => x.namespace === query);
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
        for (const fileName of files) {
            const filePath = path.join(CommandInterface.generatedPath, fileName);
            const fileNameNoExt = fileName.replace('.json', '');
            const dataArray = JSON.parse(fs.readFileSync(filePath));
            const xamlSchemaAlias = new XamlSchemaAlias(fileNameNoExt, dataArray);
            this.xamlSchemaAliases.push(xamlSchemaAlias);
        }
    }
    public static invalidate() {
        this.xamlSchemaAliases = [];
    }
}
import path from "path";
import { CommandInterface } from "../bridge";
import { Configuration } from "../configuration";
import { XamlSchemaAlias } from "./types";


export class SchemaController {
    private static microsoftMauiSchemaName: string = "Microsoft.Maui.Controls";
    private static xamlSchemaAliases: XamlSchemaAlias[] = [];

    public static loadXamlSchemaAliases() {
        if (this.xamlSchemaAliases.length > 0) 
            return;
        
        const fs = require('fs');
        if (fs.existsSync(CommandInterface.generatedPath) === false)
            return;

        fs.readdirSync(CommandInterface.generatedPath).forEach((fileName: string) => {
            const filePath = path.join(CommandInterface.generatedPath, fileName);
            const fileNameNoExt = fileName.replace('.json', '');
            const dataArray = JSON.parse(fs.readFileSync(filePath));
            const xamlSchemaAlias = new XamlSchemaAlias(fileNameNoExt, dataArray);
            this.xamlSchemaAliases.push(xamlSchemaAlias);
        });
    }
    public static generateXamlSchemaAliases() {
        if (this.xamlSchemaAliases.length > 0) 
            return;

        const projectPath = Configuration.project?.path;
        if (projectPath === undefined)
            return;
        
        CommandInterface.xamlSchema(projectPath, (succeeded: boolean) => {
            this.loadXamlSchemaAliases();
        });
    }
    public static xamlAliasByName(name: string | undefined): any[] { 
        const query = name ?? this.microsoftMauiSchemaName;
        const schema = this.xamlSchemaAliases.find(x => x.namespace === query);

        if (schema === undefined) 
            return [];

        return schema.types;
    }
}